using Microsoft.EntityFrameworkCore;
using Models.DTO.Accessories;
using Models.Models;
using Service.Services.Common;

namespace Service.Services.Accessories.Implementations;

public sealed partial class AccessoryService
{
    private const string DisposedVehicleStatus = "Disposed";
    private const string LiquidatedVehicleStatus = "Liquidated";

    private static readonly HashSet<string> GlobalRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "executivemanagement",
        "manager"
    };

    private static readonly HashSet<string> BranchRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "branchassetaccountant"
    };

    private static readonly HashSet<string> AccessoryWriteRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "executivemanagement"
    };

    private static readonly HashSet<string> BranchStockWriteRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "branchassetaccountant",
        "manager",
        "executivemanagement"
    };

    private static readonly HashSet<string> PurchaseRequestCreateRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "branchassetaccountant",
        "manager",
        "executivemanagement"
    };

    private static readonly HashSet<string> PurchaseRequestApproveRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "branchassetaccountant",
        "manager",
        "executivemanagement"
    };

    private static readonly HashSet<string> GoodsReceiptRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "branchassetaccountant",
        "manager",
        "executivemanagement"
    };

    private static readonly HashSet<string> RequirementWriteRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "branchassetaccountant",
        "manager",
        "executivemanagement"
    };

    private static readonly HashSet<string> IssueRoles = new(StringComparer.OrdinalIgnoreCase)
    {
        "operator",
        "branchassetaccountant",
        "manager",
        "executivemanagement"
    };

    private static readonly HashSet<string> AllowedAccessoryTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Reusable",
        "Consumable",
        "Fixed"
    };

    private static readonly HashSet<string> AllowedTransactionTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "IMPORT",
        "ISSUE",
        "RETURN",
        "DAMAGED",
        "LOST",
        "ADJUST"
    };

    private static readonly HashSet<string> AllowedReferenceTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "PURCHASE_REQUEST",
        "PURCHASE_RECEIPT",
        "ISSUE",
        "RETURN",
        "DAMAGED",
        "LOST",
        "ADJUST"
    };

    private static readonly HashSet<string> AllowedPurchaseRequestStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Approved",
        "Rejected",
        "PartiallyReceived",
        "Received",
        "Cancelled"
    };

    private static readonly HashSet<string> AllowedGoodsReceiptStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Draft",
        "Completed",
        "Cancelled"
    };

    private readonly CarManagerContext _context;

    public AccessoryService(CarManagerContext context)
    {
        _context = context;
    }

    private async Task<ServiceResult<AccessScope>> ResolveAccessScopeAsync(int actorUserId, IReadOnlyCollection<string> roles, bool requireAnyAccess)
    {
        if (roles == null || roles.Count == 0)
        {
            return ServiceResult<AccessScope>.Fail(403, "Role is required.");
        }

        var normalizedRoles = roles
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Select(NormalizeRole)
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        if (requireAnyAccess && !normalizedRoles.Overlaps(GlobalRoles) && !normalizedRoles.Overlaps(BranchRoles))
        {
            return ServiceResult<AccessScope>.Fail(403, "You do not have permission to access accessory data.");
        }

        if (normalizedRoles.Overlaps(GlobalRoles))
        {
            return ServiceResult<AccessScope>.SuccessResult(new AccessScope(null));
        }

        var userBranchId = await _context.Users.AsNoTracking()
            .Where(x => x.Id == actorUserId && x.DeletedAt == null)
            .Select(x => x.BranchId)
            .FirstOrDefaultAsync();
        if (!userBranchId.HasValue)
        {
            return ServiceResult<AccessScope>.Fail(400, "User is not assigned to a branch.");
        }

        return ServiceResult<AccessScope>.SuccessResult(new AccessScope(userBranchId.Value));
    }

    private async Task<ServiceResult<int?>> ResolveRequestedBranchIdAsync(AccessScope scope, int? requestedBranchId)
    {
        if (scope.RestrictedBranchId.HasValue)
        {
            if (requestedBranchId.HasValue && requestedBranchId.Value != scope.RestrictedBranchId.Value)
            {
                return ServiceResult<int?>.Fail(403, "You can only work with your own branch.");
            }

            return ServiceResult<int?>.SuccessResult(scope.RestrictedBranchId.Value);
        }

        if (requestedBranchId.HasValue)
        {
            var branchExists = await _context.Branches.AsNoTracking().AnyAsync(x => x.Id == requestedBranchId.Value && x.DeletedAt == null);
            if (!branchExists)
            {
                return ServiceResult<int?>.Fail(404, "Branch not found.");
            }
        }

        return ServiceResult<int?>.SuccessResult(requestedBranchId);
    }

    private static bool HasAnyRole(IReadOnlyCollection<string> roles, HashSet<string> allowedRoles)
    {
        return roles.Where(x => !string.IsNullOrWhiteSpace(x)).Select(NormalizeRole).Any(allowedRoles.Contains);
    }

    private async Task ExecuteInTransactionAsync(Func<Task> operation)
    {
        var strategy = _context.Database.CreateExecutionStrategy();
        await strategy.ExecuteAsync(async () =>
        {
            await using var tx = await _context.Database.BeginTransactionAsync();
            try
            {
                await operation();
                await tx.CommitAsync();
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        });
    }

    private static string NormalizeAccessoryType(string type)
        => AllowedAccessoryTypes.First(x => x.Equals(type.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string NormalizeTransactionType(string transactionType)
        => AllowedTransactionTypes.First(x => x.Equals(transactionType.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string NormalizeReferenceType(string referenceType)
        => AllowedReferenceTypes.First(x => x.Equals(referenceType.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string NormalizePurchaseRequestStatus(string status)
        => AllowedPurchaseRequestStatuses.First(x => x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string NormalizeGoodsReceiptStatus(string status)
        => AllowedGoodsReceiptStatuses.First(x => x.Equals(status.Trim(), StringComparison.OrdinalIgnoreCase));

    private static string NormalizeRole(string role)
    {
        return role.Replace(" ", string.Empty)
            .Replace("_", string.Empty)
            .Replace("-", string.Empty)
            .Trim()
            .ToLowerInvariant();
    }

    private static bool IsDisposed(string? status)
    {
        return string.Equals(status, DisposedVehicleStatus, StringComparison.OrdinalIgnoreCase)
            || string.Equals(status, LiquidatedVehicleStatus, StringComparison.OrdinalIgnoreCase);
    }

    private sealed record AccessScope(int? RestrictedBranchId);
}
