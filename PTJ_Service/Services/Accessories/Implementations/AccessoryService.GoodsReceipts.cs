using Microsoft.EntityFrameworkCore;
using Models.DTO.Accessories;
using Models.Models;
using Service.Services.Common;

namespace Service.Services.Accessories.Implementations;

public sealed partial class AccessoryService
{
    public async Task<ServiceResult<List<AccessoryGoodsReceiptDto>>> GetGoodsReceiptsAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int? branchId,
        int? purchaseRequestId,
        string? status,
        int? page,
        int? pageSize)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<List<AccessoryGoodsReceiptDto>>.Fail(access.StatusCode, access.Message!);
        }

        if (!string.IsNullOrWhiteSpace(status) && !AllowedGoodsReceiptStatuses.Contains(status.Trim()))
        {
            return ServiceResult<List<AccessoryGoodsReceiptDto>>.Fail(400, "Invalid goods receipt status.");
        }

        var scopedBranchId = await ResolveRequestedBranchIdAsync(access.Data!, branchId);
        if (!scopedBranchId.Success)
        {
            return ServiceResult<List<AccessoryGoodsReceiptDto>>.Fail(scopedBranchId.StatusCode, scopedBranchId.Message!);
        }

        var query = _context.AccessoryGoodsReceipts.AsNoTracking()
            .Include(x => x.Branch)
            .Include(x => x.PurchaseRequest)
            .Include(x => x.ReceivedByNavigation)
            .Include(x => x.AccessoryGoodsReceiptDetails)
                .ThenInclude(x => x.Accessory)
            .AsQueryable();

        if (scopedBranchId.Data.HasValue)
        {
            query = query.Where(x => x.BranchId == scopedBranchId.Data.Value);
        }

        if (purchaseRequestId.HasValue)
        {
            query = query.Where(x => x.PurchaseRequestId == purchaseRequestId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            query = query.Where(x => x.Status == NormalizeGoodsReceiptStatus(status));
        }

        query = query.OrderByDescending(x => x.ReceiptDate).ThenByDescending(x => x.Id);

        if (page.HasValue && pageSize.HasValue && page > 0 && pageSize > 0)
        {
            query = query.Skip((page.Value - 1) * pageSize.Value).Take(pageSize.Value);
        }

        var items = await query.ToListAsync();
        return ServiceResult<List<AccessoryGoodsReceiptDto>>.SuccessResult(items.Select(MapGoodsReceipt).ToList());
    }

    public async Task<ServiceResult<AccessoryGoodsReceiptDto>> GetGoodsReceiptByIdAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(access.StatusCode, access.Message!);
        }

        var entity = await LoadGoodsReceiptAsync(id, asNoTracking: true);
        if (entity == null)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(404, "Goods receipt not found.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && entity.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(403, "You can only access receipts in your branch.");
        }

        return ServiceResult<AccessoryGoodsReceiptDto>.SuccessResult(MapGoodsReceipt(entity));
    }

    public async Task<ServiceResult<AccessoryGoodsReceiptDto>> CreateGoodsReceiptAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        AccessoryGoodsReceiptCreateRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, GoodsReceiptRoles))
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(403, "You do not have permission to create goods receipts.");
        }

        var purchaseRequest = await LoadPurchaseRequestAsync(request.PurchaseRequestId, asNoTracking: false);
        if (purchaseRequest == null)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(404, "Purchase request not found.");
        }

        if (purchaseRequest.Status is not ("Approved" or "PartiallyReceived"))
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(400, "Goods receipt can only be created from approved or partially received requests.");
        }

        var branchIdResult = await ResolveRequestedBranchIdAsync(access.Data!, request.BranchId ?? purchaseRequest.BranchId);
        if (!branchIdResult.Success || !branchIdResult.Data.HasValue)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(branchIdResult.StatusCode, branchIdResult.Message!);
        }

        if (branchIdResult.Data.Value != purchaseRequest.BranchId)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(400, "Goods receipt branch must match purchase request branch.");
        }

        var validation = ValidateGoodsReceiptDetails(request.Details);
        if (!validation.Success)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(validation.StatusCode, validation.Message!);
        }

        var quantityValidation = await ValidateReceiptQuantitiesAsync(purchaseRequest, request.Details, null);
        if (!quantityValidation.Success)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(quantityValidation.StatusCode, quantityValidation.Message!);
        }

        var now = DateTime.UtcNow;
        var entity = new AccessoryGoodsReceipt
        {
            PurchaseRequestId = purchaseRequest.Id,
            BranchId = branchIdResult.Data.Value,
            ReceivedBy = actorUserId,
            ReceiptDate = now,
            Status = "Draft",
            Notes = request.Notes?.Trim(),
            CreatedAt = now,
            AccessoryGoodsReceiptDetails = request.Details.Select(detail => new AccessoryGoodsReceiptDetail
            {
                AccessoryId = detail.AccessoryId,
                ReceivedQuantity = detail.ReceivedQuantity,
                ActualUnitPrice = detail.ActualUnitPrice
            }).ToList()
        };

        _context.AccessoryGoodsReceipts.Add(entity);
        await _context.SaveChangesAsync();

        var result = await GetGoodsReceiptByIdAsync(actorUserId, roles, entity.Id);
        return result.Success
            ? ServiceResult<AccessoryGoodsReceiptDto>.SuccessResult(result.Data!, 201)
            : result;
    }

    public async Task<ServiceResult<AccessoryGoodsReceiptDto>> CompleteGoodsReceiptAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        AccessoryGoodsReceiptCompleteRequestDto request)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, GoodsReceiptRoles))
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(403, "You do not have permission to complete goods receipts.");
        }

        var receipt = await LoadGoodsReceiptAsync(id, asNoTracking: false);
        if (receipt == null)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(404, "Goods receipt not found.");
        }

        if (receipt.Status != "Draft")
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(400, "Only draft goods receipts can be completed.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && receipt.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(403, "You can only complete receipts in your branch.");
        }

        if (receipt.PurchaseRequest.Status is not ("Approved" or "PartiallyReceived"))
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(400, "Purchase request is not eligible for receiving.");
        }

        var detailValidation = ValidateGoodsReceiptDetails(request.Details);
        if (!detailValidation.Success)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(detailValidation.StatusCode, detailValidation.Message!);
        }

        var quantityValidation = await ValidateReceiptQuantitiesAsync(receipt.PurchaseRequest, request.Details, receipt.Id);
        if (!quantityValidation.Success)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(quantityValidation.StatusCode, quantityValidation.Message!);
        }

        await ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;
            _context.AccessoryGoodsReceiptDetails.RemoveRange(receipt.AccessoryGoodsReceiptDetails);
            receipt.AccessoryGoodsReceiptDetails = request.Details.Select(detail => new AccessoryGoodsReceiptDetail
            {
                ReceiptId = receipt.Id,
                AccessoryId = detail.AccessoryId,
                ReceivedQuantity = detail.ReceivedQuantity,
                ActualUnitPrice = detail.ActualUnitPrice
            }).ToList();
            receipt.Status = "Completed";
            receipt.Notes = request.Notes?.Trim() ?? receipt.Notes;
            receipt.ReceiptDate = now;
            receipt.ReceivedBy = actorUserId;
            await _context.SaveChangesAsync();

            foreach (var detail in receipt.AccessoryGoodsReceiptDetails)
            {
                var stock = await _context.BranchAccessoryStocks
                    .FirstOrDefaultAsync(x => x.BranchId == receipt.BranchId && x.AccessoryId == detail.AccessoryId);

                if (stock == null)
                {
                    stock = new BranchAccessoryStock
                    {
                        BranchId = receipt.BranchId,
                        AccessoryId = detail.AccessoryId,
                        QuantityInStock = 0,
                        MinimumStock = null,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _context.BranchAccessoryStocks.Add(stock);
                }

                stock.QuantityInStock += detail.ReceivedQuantity;
                stock.UpdatedAt = now;

                _context.AccessoryTransactions.Add(new AccessoryTransaction
                {
                    AccessoryId = detail.AccessoryId,
                    BranchId = receipt.BranchId,
                    TransactionType = "IMPORT",
                    ReferenceType = "PURCHASE_RECEIPT",
                    ReferenceId = receipt.Id,
                    Quantity = detail.ReceivedQuantity,
                    UnitPrice = detail.ActualUnitPrice,
                    Notes = receipt.Notes,
                    PerformedBy = actorUserId,
                    TransactionDate = now
                });
            }

            receipt.PurchaseRequest.Status = await ResolvePurchaseRequestReceiptStatusAsync(receipt.PurchaseRequest.Id, receipt.PurchaseRequest);
            receipt.PurchaseRequest.UpdatedAt = now;
            await _context.SaveChangesAsync();
        });

        return await GetGoodsReceiptByIdAsync(actorUserId, roles, receipt.Id);
    }

    public async Task<ServiceResult<AccessoryGoodsReceiptDto>> CancelGoodsReceiptAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id,
        string? notes)
    {
        var access = await ResolveAccessScopeAsync(actorUserId, roles, requireAnyAccess: true);
        if (!access.Success)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(access.StatusCode, access.Message!);
        }

        if (!HasAnyRole(roles, GoodsReceiptRoles))
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(403, "You do not have permission to cancel goods receipts.");
        }

        var receipt = await LoadGoodsReceiptAsync(id, asNoTracking: false);
        if (receipt == null)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(404, "Goods receipt not found.");
        }

        if (receipt.Status != "Draft")
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(400, "Only draft goods receipts can be cancelled.");
        }

        if (access.Data!.RestrictedBranchId.HasValue && receipt.BranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<AccessoryGoodsReceiptDto>.Fail(403, "You can only cancel receipts in your branch.");
        }

        receipt.Status = "Cancelled";
        receipt.Notes = notes?.Trim() ?? receipt.Notes;
        await _context.SaveChangesAsync();
        return await GetGoodsReceiptByIdAsync(actorUserId, roles, receipt.Id);
    }
}
