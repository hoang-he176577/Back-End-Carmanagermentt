using Data.Repositories.DisposalProposals.Interfaces;
using Models.DTO.DisposalProposals;
using Models.Models;
using Service.Services.Common;
using Service.Services.DisposalProposals.Interfaces;

namespace Service.Services.DisposalProposals.Implementations;

public sealed class DisposalProposalService : IDisposalProposalService
{
    private const string PendingStatus = "Pending";
    private const string ApprovedStatus = "Approved";
    private const string RejectedStatus = "Rejected";
    private const string DisposedVehicleStatus = "Disposed";
    private const string LiquidatedVehicleStatus = "Liquidated";
    private const string AssetChangeTypeDisposal = "DISPOSAL";

    private static readonly HashSet<string> GlobalRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "executivemanagement"
    };

    private static readonly HashSet<string> ReadRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "branchassetaccountant",
        "executivemanagement"
    };

    private static readonly HashSet<string> CreateRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator"
    };

    private readonly IDisposalProposalRepository _repository;

    public DisposalProposalService(IDisposalProposalRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<DisposalProposalPagedResultDto>> GetListAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        DisposalProposalListQueryDto query)
    {
        query ??= new DisposalProposalListQueryDto();

        var access = await ResolveReadAccessScopeAsync(actorUserId, roles);
        if (!access.Success)
        {
            return ServiceResult<DisposalProposalPagedResultDto>.Fail(access.StatusCode, access.Message!);
        }

        if (query.FromDate.HasValue && query.ToDate.HasValue && query.FromDate.Value > query.ToDate.Value)
        {
            return ServiceResult<DisposalProposalPagedResultDto>.Fail(400, "fromDate must be less than or equal to toDate.");
        }

        var page = query.Page.GetValueOrDefault(1);
        var pageSize = query.PageSize.GetValueOrDefault(20);
        if (page <= 0)
        {
            page = 1;
        }

        if (pageSize <= 0)
        {
            pageSize = 20;
        }
        else if (pageSize > 200)
        {
            pageSize = 200;
        }

        var data = await _repository.GetListAsync(
            query.Status,
            query.VehicleId,
            query.ProposerId,
            query.FromDate,
            query.ToDate,
            access.Data!.RestrictedBranchId,
            page,
            pageSize);

        return ServiceResult<DisposalProposalPagedResultDto>.SuccessResult(new DisposalProposalPagedResultDto
        {
            Items = data.Items,
            TotalCount = data.TotalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    public async Task<ServiceResult<DisposalProposalDto>> GetByIdAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id)
    {
        var access = await ResolveReadAccessScopeAsync(actorUserId, roles);
        if (!access.Success)
        {
            return ServiceResult<DisposalProposalDto>.Fail(access.StatusCode, access.Message!);
        }

        var dto = await _repository.GetByIdAsync(id, access.Data!.RestrictedBranchId);
        if (dto == null)
        {
            return ServiceResult<DisposalProposalDto>.Fail(404, "Disposal proposal not found.");
        }

        return ServiceResult<DisposalProposalDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<List<DisposalProposalDto>>> GetByVehicleIdAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleId)
    {
        var access = await ResolveReadAccessScopeAsync(actorUserId, roles);
        if (!access.Success)
        {
            return ServiceResult<List<DisposalProposalDto>>.Fail(access.StatusCode, access.Message!);
        }

        var vehicle = await _repository.GetVehicleByIdAsync(vehicleId);
        if (vehicle == null)
        {
            return ServiceResult<List<DisposalProposalDto>>.Fail(404, "Vehicle not found.");
        }

        if (access.Data!.RestrictedBranchId.HasValue &&
            vehicle.CurrentBranchId != access.Data.RestrictedBranchId.Value)
        {
            return ServiceResult<List<DisposalProposalDto>>.Fail(403, "You can only access proposals in your branch.");
        }

        var items = await _repository.GetByVehicleIdAsync(vehicleId, access.Data.RestrictedBranchId);
        return ServiceResult<List<DisposalProposalDto>>.SuccessResult(items);
    }

    public async Task<ServiceResult<DisposalProposalDto>> CreateAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        DisposalProposalCreateRequestDto request)
    {
        if (request == null)
        {
            return ServiceResult<DisposalProposalDto>.Fail(400, "Request body is required.");
        }

        var roleCheck = ResolveRoleAccess(roles);
        if (!roleCheck.HasRoles)
        {
            return ServiceResult<DisposalProposalDto>.Fail(403, "Role is required.");
        }

        if (!roleCheck.NormalizedRoles.Overlaps(CreateRoles))
        {
            return ServiceResult<DisposalProposalDto>.Fail(403, "You do not have permission to create disposal proposals.");
        }

        if (request.ProposedPrice < 0)
        {
            return ServiceResult<DisposalProposalDto>.Fail(400, "ProposedPrice must be >= 0.");
        }

        if (string.IsNullOrWhiteSpace(request.Reason))
        {
            return ServiceResult<DisposalProposalDto>.Fail(400, "Reason is required.");
        }

        var vehicle = await _repository.GetVehicleByIdAsync(request.VehicleId);
        if (vehicle == null)
        {
            return ServiceResult<DisposalProposalDto>.Fail(404, "Vehicle not found.");
        }

        if (IsDisposed(vehicle.Status))
        {
            return ServiceResult<DisposalProposalDto>.Fail(409, "Vehicle is already disposed/liquidated.");
        }

        if (!roleCheck.NormalizedRoles.Overlaps(GlobalRoles))
        {
            var userBranchId = await _repository.GetUserBranchIdAsync(actorUserId);
            if (!userBranchId.HasValue)
            {
                return ServiceResult<DisposalProposalDto>.Fail(400, "User is not assigned to a branch.");
            }

            if (vehicle.CurrentBranchId != userBranchId.Value)
            {
                return ServiceResult<DisposalProposalDto>.Fail(403, "You can only create proposals for vehicles in your branch.");
            }
        }

        var hasPending = await _repository.HasPendingProposalAsync(request.VehicleId);
        if (hasPending)
        {
            return ServiceResult<DisposalProposalDto>.Fail(409, "A pending disposal proposal already exists for this vehicle.");
        }

        var now = DateTime.UtcNow;
        var entity = new DisposalProposal
        {
            VehicleId = request.VehicleId,
            ProposerId = actorUserId,
            ManagerId = null,
            CreatedDate = DateOnly.FromDateTime(now),
            ApprovedDate = null,
            Status = PendingStatus,
            ProposedPrice = request.ProposedPrice,
            Reason = request.Reason.Trim(),
            CreatedAt = now,
            UpdatedAt = now
        };

        await _repository.AddProposalAsync(entity);
        await _repository.SaveChangesAsync();

        var created = await _repository.GetByIdAsync(entity.Id, null);
        if (created == null)
        {
            return ServiceResult<DisposalProposalDto>.Fail(500, "Failed to load created disposal proposal.");
        }

        return ServiceResult<DisposalProposalDto>.SuccessResult(created, 201);
    }

    public async Task<ServiceResult<DisposalProposalDto>> ApproveAsync(
        int proposalId,
        int managerUserId,
        DisposalProposalApproveRequestDto? request)
    {
        request ??= new DisposalProposalApproveRequestDto();

        var entity = await _repository.GetEntityByIdAsync(proposalId);
        if (entity == null)
        {
            return ServiceResult<DisposalProposalDto>.Fail(404, "Disposal proposal not found.");
        }

        if (!string.Equals(entity.Status, PendingStatus, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<DisposalProposalDto>.Fail(409, "Only pending proposals can be approved.");
        }

        if (entity.Vehicle == null || !entity.VehicleId.HasValue)
        {
            return ServiceResult<DisposalProposalDto>.Fail(400, "Proposal does not have a valid vehicle.");
        }

        if (IsDisposed(entity.Vehicle.Status))
        {
            return ServiceResult<DisposalProposalDto>.Fail(409, "Vehicle is already disposed/liquidated.");
        }

        var hasActiveAccessories = await _repository.HasActiveAccessoriesAsync(entity.VehicleId.Value);
        if (hasActiveAccessories)
        {
            return ServiceResult<DisposalProposalDto>.Fail(409, "Vehicle still has active accessories. Please process all installed accessories before approving disposal.");
        }

        if (request.AccountantId.HasValue && !await _repository.UserExistsAsync(request.AccountantId.Value))
        {
            return ServiceResult<DisposalProposalDto>.Fail(400, "Accountant user not found.");
        }

        await _repository.ExecuteInTransactionAsync(async () =>
        {
            var now = DateTime.UtcNow;

            entity.Status = ApprovedStatus;
            entity.ManagerId = managerUserId;
            entity.ApprovedDate = DateOnly.FromDateTime(now);
            entity.UpdatedAt = now;

            entity.Vehicle.Status = DisposedVehicleStatus;
            entity.Vehicle.CurrentDriverId = null;
            entity.Vehicle.UpdatedAt = now;

            var noteText = string.IsNullOrWhiteSpace(request.Notes)
                ? null
                : request.Notes.Trim();

            var combinedReason = string.IsNullOrWhiteSpace(noteText)
                ? entity.Reason
                : $"{entity.Reason} | {noteText}";

            var changeDate = request.ChangeDate ?? DateOnly.FromDateTime(now);

            // Use proposed price as disposal delta because it is the approved liquidation value.
            var amountChange = entity.ProposedPrice ?? entity.Vehicle.CurrentValue ?? 0m;

            await _repository.AddAssetChangeLogAsync(new AssetChangeLog
            {
                VehicleId = entity.VehicleId,
                ChangeType = AssetChangeTypeDisposal,
                ChangeDate = changeDate,
                AmountChange = amountChange,
                Reason = combinedReason,
                AccountantId = request.AccountantId
            });

            await _repository.SaveChangesAsync();
        });

        var approved = await _repository.GetByIdAsync(proposalId, null);
        if (approved == null)
        {
            return ServiceResult<DisposalProposalDto>.Fail(500, "Failed to load approved proposal.");
        }

        return ServiceResult<DisposalProposalDto>.SuccessResult(approved);
    }

    public async Task<ServiceResult<DisposalProposalDto>> RejectAsync(
        int proposalId,
        int managerUserId,
        DisposalProposalRejectRequestDto? request)
    {
        var entity = await _repository.GetEntityByIdAsync(proposalId);
        if (entity == null)
        {
            return ServiceResult<DisposalProposalDto>.Fail(404, "Disposal proposal not found.");
        }

        if (!string.Equals(entity.Status, PendingStatus, StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<DisposalProposalDto>.Fail(409, "Only pending proposals can be rejected.");
        }

        entity.Status = RejectedStatus;
        entity.ManagerId = managerUserId;
        entity.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(request?.RejectNote))
        {
            entity.Reason = $"{entity.Reason} | Rejected note: {request.RejectNote.Trim()}";
        }

        await _repository.SaveChangesAsync();

        var rejected = await _repository.GetByIdAsync(proposalId, null);
        if (rejected == null)
        {
            return ServiceResult<DisposalProposalDto>.Fail(500, "Failed to load rejected proposal.");
        }

        return ServiceResult<DisposalProposalDto>.SuccessResult(rejected);
    }

    private async Task<ServiceResult<AccessScope>> ResolveReadAccessScopeAsync(int actorUserId, IReadOnlyCollection<string> roles)
    {
        var roleAccess = ResolveRoleAccess(roles);
        if (!roleAccess.HasRoles)
        {
            return ServiceResult<AccessScope>.Fail(403, "Role is required.");
        }

        if (!roleAccess.NormalizedRoles.Overlaps(ReadRoles))
        {
            return ServiceResult<AccessScope>.Fail(403, "You do not have permission to access disposal proposals.");
        }

        if (roleAccess.NormalizedRoles.Overlaps(GlobalRoles))
        {
            return ServiceResult<AccessScope>.SuccessResult(new AccessScope(null));
        }

        var userBranchId = await _repository.GetUserBranchIdAsync(actorUserId);
        if (!userBranchId.HasValue)
        {
            return ServiceResult<AccessScope>.Fail(400, "User is not assigned to a branch.");
        }

        return ServiceResult<AccessScope>.SuccessResult(new AccessScope(userBranchId.Value));
    }

    private static RoleAccess ResolveRoleAccess(IReadOnlyCollection<string> roles)
    {
        if (roles == null || roles.Count == 0)
        {
            return new RoleAccess(false, new HashSet<string>(StringComparer.OrdinalIgnoreCase));
        }

        var normalized = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeRole)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        return new RoleAccess(normalized.Count > 0, normalized);
    }

    private static bool IsDisposed(string? status)
    {
        return string.Equals(status, DisposedVehicleStatus, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, LiquidatedVehicleStatus, StringComparison.OrdinalIgnoreCase);
    }

    private static string NormalizeRole(string role)
    {
        return role.Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Trim()
            .ToLowerInvariant();
    }

    private sealed record AccessScope(int? RestrictedBranchId);

    private sealed record RoleAccess(bool HasRoles, HashSet<string> NormalizedRoles);
}
