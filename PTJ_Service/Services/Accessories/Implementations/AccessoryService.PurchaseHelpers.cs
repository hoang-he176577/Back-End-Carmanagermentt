using Microsoft.EntityFrameworkCore;
using Models.DTO.Accessories;
using Models.Models;
using Service.Services.Common;

namespace Service.Services.Accessories.Implementations;

public sealed partial class AccessoryService
{
    private async Task<ServiceResult<bool>> ValidatePurchaseRequestDetailsAsync(List<AccessoryPurchaseRequestDetailRequestDto> details)
    {
        if (details == null || details.Count == 0)
        {
            return ServiceResult<bool>.Fail(400, "At least one detail line is required.");
        }

        if (details.Any(x => x.RequestedQuantity <= 0))
        {
            return ServiceResult<bool>.Fail(400, "Requested quantity must be greater than 0.");
        }

        if (details.GroupBy(x => x.AccessoryId).Any(x => x.Count() > 1))
        {
            return ServiceResult<bool>.Fail(400, "Accessory lines must be unique within the request.");
        }

        var accessoryIds = details.Select(x => x.AccessoryId).Distinct().ToList();
        var existingCount = await _context.Accessories.CountAsync(x => accessoryIds.Contains(x.Id) && x.DeletedAt == null);
        if (existingCount != accessoryIds.Count)
        {
            return ServiceResult<bool>.Fail(404, "One or more accessories were not found.");
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    private static ServiceResult<bool> ValidateGoodsReceiptDetails(List<AccessoryGoodsReceiptDetailRequestDto> details)
    {
        if (details == null || details.Count == 0)
        {
            return ServiceResult<bool>.Fail(400, "At least one receipt detail line is required.");
        }

        if (details.Any(x => x.ReceivedQuantity <= 0))
        {
            return ServiceResult<bool>.Fail(400, "Received quantity must be greater than 0.");
        }

        if (details.GroupBy(x => x.AccessoryId).Any(x => x.Count() > 1))
        {
            return ServiceResult<bool>.Fail(400, "Accessory lines must be unique within the receipt.");
        }

        if (details.Any(x => string.IsNullOrWhiteSpace(x.StockCondition)))
        {
            return ServiceResult<bool>.Fail(400, "StockCondition is required for all receipt lines.");
        }

        if (details.Any(x => !AllowedStockConditions.Contains(x.StockCondition.Trim())))
        {
            return ServiceResult<bool>.Fail(400, "StockCondition must be NEW, USED, or DAMAGED.");
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    private async Task<ServiceResult<bool>> ValidateReceiptQuantitiesAsync(
        AccessoryPurchaseRequest purchaseRequest,
        List<AccessoryGoodsReceiptDetailRequestDto> details,
        int? receiptIdToExclude)
    {
        var requestDetails = purchaseRequest.AccessoryPurchaseRequestDetails.ToDictionary(x => x.AccessoryId);
        foreach (var detail in details)
        {
            if (!requestDetails.TryGetValue(detail.AccessoryId, out var requestDetail))
            {
                return ServiceResult<bool>.Fail(400, "Receipt contains accessory not present in the purchase request.");
            }

            var approvedQuantity = requestDetail.ApprovedQuantity ?? 0;
            if (approvedQuantity <= 0)
            {
                return ServiceResult<bool>.Fail(400, "Cannot receive accessory lines with approved quantity 0.");
            }

            var alreadyReceived = await _context.AccessoryGoodsReceiptDetails
                .Where(x =>
                    x.AccessoryId == detail.AccessoryId &&
                    x.Receipt.PurchaseRequestId == purchaseRequest.Id &&
                    x.Receipt.Status == "Completed" &&
                    (!receiptIdToExclude.HasValue || x.ReceiptId != receiptIdToExclude.Value))
                .SumAsync(x => (int?)x.ReceivedQuantity) ?? 0;

            if (alreadyReceived + detail.ReceivedQuantity > approvedQuantity)
            {
                return ServiceResult<bool>.Fail(400, "Received quantity exceeds approved quantity.");
            }
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    private async Task<AccessoryPurchaseRequest?> LoadPurchaseRequestAsync(int id, bool asNoTracking)
    {
        var query = _context.AccessoryPurchaseRequests
            .Include(x => x.Branch)
            .Include(x => x.Requester)
            .Include(x => x.ApprovedBy)
            .Include(x => x.AccessoryPurchaseRequestDetails)
                .ThenInclude(x => x.Accessory)
            .Include(x => x.AccessoryGoodsReceipts)
                .ThenInclude(x => x.AccessoryGoodsReceiptDetails)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    private async Task<AccessoryGoodsReceipt?> LoadGoodsReceiptAsync(int id, bool asNoTracking)
    {
        var query = _context.AccessoryGoodsReceipts
            .Include(x => x.Branch)
            .Include(x => x.PurchaseRequest)
                .ThenInclude(x => x.AccessoryPurchaseRequestDetails)
            .Include(x => x.ReceivedByNavigation)
            .Include(x => x.AccessoryGoodsReceiptDetails)
                .ThenInclude(x => x.Accessory)
            .AsQueryable();

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        return await query.FirstOrDefaultAsync(x => x.Id == id);
    }

    private async Task<string> GeneratePurchaseRequestCodeAsync()
    {
        string code;
        do
        {
            code = $"APR-{DateTime.UtcNow:yyyyMMddHHmmssfff}";
        }
        while (await _context.AccessoryPurchaseRequests.AnyAsync(x => x.RequestCode == code));

        return code;
    }

    private async Task<string> ResolvePurchaseRequestReceiptStatusAsync(int purchaseRequestId, AccessoryPurchaseRequest currentRequest)
    {
        var approvedByAccessory = currentRequest.AccessoryPurchaseRequestDetails.ToDictionary(
            x => x.AccessoryId,
            x => x.ApprovedQuantity ?? 0);

        var receivedByAccessory = await _context.AccessoryGoodsReceiptDetails
            .Where(x => x.Receipt.PurchaseRequestId == purchaseRequestId && x.Receipt.Status == "Completed")
            .GroupBy(x => x.AccessoryId)
            .Select(x => new { AccessoryId = x.Key, Quantity = x.Sum(y => y.ReceivedQuantity) })
            .ToListAsync();

        var hasAnyReceipt = receivedByAccessory.Any(x => x.Quantity > 0);
        var allReceived = approvedByAccessory.All(entry =>
            receivedByAccessory.FirstOrDefault(x => x.AccessoryId == entry.Key)?.Quantity >= entry.Value);

        if (allReceived)
        {
            return "Received";
        }

        return hasAnyReceipt ? "PartiallyReceived" : "Approved";
    }

    private static AccessoryPurchaseRequestDto MapPurchaseRequest(AccessoryPurchaseRequest entity)
    {
        var receivedByAccessory = entity.AccessoryGoodsReceipts
            .Where(x => x.Status == "Completed")
            .SelectMany(x => x.AccessoryGoodsReceiptDetails)
            .GroupBy(x => x.AccessoryId)
            .ToDictionary(x => x.Key, x => x.Sum(y => y.ReceivedQuantity));

        return new AccessoryPurchaseRequestDto
        {
            Id = entity.Id,
            RequestCode = entity.RequestCode,
            BranchId = entity.BranchId,
            BranchName = entity.Branch.Name,
            RequesterId = entity.RequesterId,
            RequesterName = entity.Requester.Name,
            ApprovedById = entity.ApprovedById,
            ApprovedByName = entity.ApprovedBy?.Name,
            RequestDate = entity.RequestDate,
            ApprovedDate = entity.ApprovedDate,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt,
            Details = entity.AccessoryPurchaseRequestDetails
                .OrderBy(x => x.Id)
                .Select(detail => new AccessoryPurchaseRequestDetailDto
                {
                    Id = detail.Id,
                    AccessoryId = detail.AccessoryId,
                    AccessoryCode = detail.Accessory?.Code,
                    AccessoryName = detail.Accessory?.Name,
                    ImageUrl = detail.Accessory?.ImageUrl,
                    RequestedQuantity = detail.RequestedQuantity,
                    ApprovedQuantity = detail.ApprovedQuantity,
                    ReceivedQuantity = receivedByAccessory.TryGetValue(detail.AccessoryId, out var receivedQuantity) ? receivedQuantity : 0,
                    EstimatedUnitPrice = detail.EstimatedUnitPrice,
                    Notes = detail.Notes
                })
                .ToList()
        };
    }

    private static AccessoryGoodsReceiptDto MapGoodsReceipt(AccessoryGoodsReceipt entity)
    {
        return new AccessoryGoodsReceiptDto
        {
            Id = entity.Id,
            PurchaseRequestId = entity.PurchaseRequestId,
            PurchaseRequestCode = entity.PurchaseRequest?.RequestCode,
            PurchaseRequestStatus = entity.PurchaseRequest?.Status,
            BranchId = entity.BranchId,
            BranchName = entity.Branch.Name,
            ReceivedBy = entity.ReceivedBy,
            ReceivedByName = entity.ReceivedByNavigation.Name,
            ReceiptDate = entity.ReceiptDate,
            Status = entity.Status,
            Notes = entity.Notes,
            CreatedAt = entity.CreatedAt,
            Details = entity.AccessoryGoodsReceiptDetails
                .OrderBy(x => x.Id)
                .Select(detail => new AccessoryGoodsReceiptDetailDto
                {
                    Id = detail.Id,
                    AccessoryId = detail.AccessoryId,
                    AccessoryCode = detail.Accessory.Code,
                    AccessoryName = detail.Accessory.Name,
                    ImageUrl = detail.Accessory.ImageUrl,
                    ReceivedQuantity = detail.ReceivedQuantity,
                    ActualUnitPrice = detail.ActualUnitPrice,
                    StockCondition = detail.StockCondition
                })
                .ToList()
        };
    }
}
