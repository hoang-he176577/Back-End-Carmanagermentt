using Microsoft.EntityFrameworkCore;
using Models.DTO.Accessories;
using Models.Models;
using Service.Services.Common;

namespace Service.Services.Accessories.Implementations;

public sealed partial class AccessoryService
{
    public async Task<ServiceResult<List<AccessoryPurchaseRequestDto>>> GetPurchaseRequestsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? branchId,
        string? status,
        int? page,
        int? pageSize)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<List<AccessoryPurchaseRequestDto>>.Fail(access.StatusCode, access.Message!);
        }

        if (!string.IsNullOrWhiteSpace(status) && !AllowedPurchaseRequestStatuses.Contains(status.Trim()))
        {
            return ServiceResult<List<AccessoryPurchaseRequestDto>>.Fail(400, "Invalid purchase request status.");
        }

        var scopedBranchId = await ResolveRequestedBranchIdAsync(access.Data!, branchId);
        if (!scopedBranchId.Success)
        {
            return ServiceResult<List<AccessoryPurchaseRequestDto>>.Fail(scopedBranchId.StatusCode, scopedBranchId.Message!);
        }

        var query = _context.AccessoryPurchaseRequests.AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.Requester)
            .Include(x => x.ApprovedBy)
            .Include(x => x.AccessoryPurchaseRequestDetails)
                .ThenInclude(x => x.Accessory)
            .Include(x => x.AccessoryGoodsReceipts)
                .ThenInclude(x => x.AccessoryGoodsReceiptDetails)
            .AsQueryable();

        if (scopedBranchId.Data.HasValue)
        {
            query = query.Where(x => x.BranchId == scopedBranchId.Data.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == NormalizePurchaseRequestStatus(status));
        }

        query = query.OrderByDescending(x => x.RequestDate).ThenByDescending(x => x.Id);

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var items = await query.ToListAsync();
        return ServiceResult<List<AccessoryPurchaseRequestDto>>.SuccessResult(items.Select(MapPurchaseRequest).ToList());
    }

    public async Task<ServiceResult<AccessoryPurchaseRequestDto>> GetPurchaseRequestByIdAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(access.StatusCode, access.Message!);
        }

        var entity = await LoadPurchaseRequestAsync(id, asNoTracking: true);
        if (entity == null)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(404, "Purchase request not found.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && entity.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(403, "You can only access purchase requests in your branch.");
        }

        return ServiceResult<AccessoryPurchaseRequestDto>.SuccessResult(MapPurchaseRequest(entity));
    }

    public async Task<ServiceResult<AccessoryPurchaseRequestDto>> CreatePurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        AccessoryPurchaseRequestCreateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, PurchaseRequestCreateRoles))
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(403, "You do not have permission to create purchase requests.");
        }

        var branchIdResult = await ResolveRequestedBranchIdAsync(access.Data!, request.BranchId);
        if (!branchIdResult.Success || !branchIdResult.Data.HasValue)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(branchIdResult.StatusCode, branchIdResult.Message!);
        }

        var validation = await ValidatePurchaseRequestDetailsAsync(request.Details);
        if (!validation.Success)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(validation.StatusCode, validation.Message!);
        }

        var now = DateTime.UtcNow;
        var entity = new AccessoryPurchaseRequest
        {
            RequestCode = await GeneratePurchaseRequestCodeAsync(),
            BranchId = branchIdResult.Data.Value,
            RequesterId = actorUserId,
            RequestDate = now,
            Status = "Pending",
            Notes = request.Notes?.Trim(),
            CreatedAt = now,
            UpdatedAt = now,
            AccessoryPurchaseRequestDetails = request.Details.Select(detail => new AccessoryPurchaseRequestDetail
            {
                AccessoryId = detail.AccessoryId,
                RequestedQuantity = detail.RequestedQuantity,
                ApprovedQuantity = null,
                EstimatedUnitPrice = detail.EstimatedUnitPrice,
                Notes = detail.Notes?.Trim()
            }).ToList()
        };

        _context.AccessoryPurchaseRequests.Add(entity);
        await _context.SaveChangesAsync();

        var result = await GetPurchaseRequestByIdAsync(actorUserId, roles, entity.Id);
        return result.Success
            ? ServiceResult<AccessoryPurchaseRequestDto>.SuccessResult(result.Data!, 201)
            : result;
    }

    public async Task<ServiceResult<AccessoryPurchaseRequestDto>> UpdatePurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        AccessoryPurchaseRequestUpdateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, PurchaseRequestCreateRoles))
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(403, "You do not have permission to update purchase requests.");
        }

        var entity = await LoadPurchaseRequestAsync(id, asNoTracking: false);
        if (entity == null)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(404, "Purchase request not found.");
        }

        if (entity.Status != "Pending")
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(400, "Only pending purchase requests can be edited.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && entity.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(403, "You can only update purchase requests in your branch.");
        }

        var validation = await ValidatePurchaseRequestDetailsAsync(request.Details);
        if (!validation.Success)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(validation.StatusCode, validation.Message!);
        }

        entity.Notes = request.Notes?.Trim();
        entity.UpdatedAt = DateTime.UtcNow;

        _context.AccessoryPurchaseRequestDetails.RemoveRange(entity.AccessoryPurchaseRequestDetails);
        entity.AccessoryPurchaseRequestDetails = request.Details.Select(detail => new AccessoryPurchaseRequestDetail
        {
            RequestId = entity.Id,
            AccessoryId = detail.AccessoryId,
            RequestedQuantity = detail.RequestedQuantity,
            ApprovedQuantity = null,
            EstimatedUnitPrice = detail.EstimatedUnitPrice,
            Notes = detail.Notes?.Trim()
        }).ToList();

        await _context.SaveChangesAsync();
        return await GetPurchaseRequestByIdAsync(actorUserId, roles, entity.Id);
    }

    public async Task<ServiceResult<bool>> DeletePurchaseRequestAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<bool>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, PurchaseRequestCreateRoles))
        {
            return ServiceResult<bool>.Fail(403, "You do not have permission to delete purchase requests.");
        }

        var entity = await LoadPurchaseRequestAsync(id, asNoTracking: false);
        if (entity == null)
        {
            return ServiceResult<bool>.Fail(404, "Purchase request not found.");
        }

        if (entity.Status != "Pending")
        {
            return ServiceResult<bool>.Fail(400, "Only pending purchase requests can be deleted.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && entity.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<bool>.Fail(403, "You can only delete purchase requests in your branch.");
        }

        _context.AccessoryPurchaseRequestDetails.RemoveRange(entity.AccessoryPurchaseRequestDetails);
        _context.AccessoryPurchaseRequests.Remove(entity);
        await _context.SaveChangesAsync();
        return ServiceResult<bool>.SuccessResult(true);
    }

    public async Task<ServiceResult<AccessoryPurchaseRequestDto>> ApprovePurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        AccessoryPurchaseRequestApproveRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, PurchaseRequestApproveRoles))
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(403, "You do not have permission to approve purchase requests.");
        }

        var entity = await LoadPurchaseRequestAsync(id, asNoTracking: false);
        if (entity == null)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(404, "Purchase request not found.");
        }

        if (entity.Status != "Pending")
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(400, "Only pending purchase requests can be approved.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && entity.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(403, "You can only approve purchase requests in your branch.");
        }

        if (request.Details == null || request.Details.Count == 0)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(400, "Approval details are required.");
        }

        var detailMap = request.Details.GroupBy(x => x.AccessoryId).ToDictionary(x => x.Key, x => x.Last().ApprovedQuantity);
        foreach (var detail in entity.AccessoryPurchaseRequestDetails)
        {
            if (!detailMap.TryGetValue(detail.AccessoryId, out var approvedQuantity))
            {
                return ServiceResult<AccessoryPurchaseRequestDto>.Fail(400, "Approval details must include every requested accessory.");
            }

            if (approvedQuantity < 0 || approvedQuantity > detail.RequestedQuantity)
            {
                return ServiceResult<AccessoryPurchaseRequestDto>.Fail(400, "Approved quantity must be between 0 and requested quantity.");
            }

            detail.ApprovedQuantity = approvedQuantity;
        }

        entity.Status = "Approved";
        entity.ApprovedById = actorUserId;
        entity.ApprovedDate = DateTime.UtcNow;
        entity.Notes = request.Notes?.Trim() ?? entity.Notes;
        entity.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return await GetPurchaseRequestByIdAsync(actorUserId, roles, entity.Id);
    }

    public async Task<ServiceResult<AccessoryPurchaseRequestDto>> RejectPurchaseRequestAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        AccessoryPurchaseRequestRejectRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, PurchaseRequestApproveRoles))
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(403, "You do not have permission to reject purchase requests.");
        }

        var entity = await LoadPurchaseRequestAsync(id, asNoTracking: false);
        if (entity == null)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(404, "Purchase request not found.");
        }

        if (entity.Status != "Pending")
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(400, "Only pending purchase requests can be rejected.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && entity.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(403, "You can only reject purchase requests in your branch.");
        }

        if (string.IsNullOrWhiteSpace(request.Notes))
        {
            return ServiceResult<AccessoryPurchaseRequestDto>.Fail(400, "Reject reason is required.");
        }

        entity.Status = "Rejected";
        entity.ApprovedById = actorUserId;
        entity.ApprovedDate = DateTime.UtcNow;
        entity.Notes = request.Notes?.Trim() ?? entity.Notes;
        entity.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();

        return await GetPurchaseRequestByIdAsync(actorUserId, roles, entity.Id);
    }
}
