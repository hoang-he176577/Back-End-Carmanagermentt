using Microsoft.EntityFrameworkCore;
using Models.DTO.Accessories;
using Models.Models;
using Service.Services.Common;

namespace Service.Services.Accessories.Implementations;

public sealed partial class AccessoryService
{
    private async Task<ServiceResult<bool>> ValidateRequirementRequestAsync(VehicleAccessoryRequirementUpsertRequestDto request, int? excludeId)
    {
        if (request.RequiredQuantity <= 0)
        {
            return ServiceResult<bool>.Fail(400, "Required quantity must be greater than 0.");
        }

        var modelExists = await _context.VehicleModels.AnyAsync(x => x.Id == request.ModelId && x.DeletedAt == null);
        var accessoryExists = await _context.Accessories.AnyAsync(x => x.Id == request.AccessoryId && x.DeletedAt == null);
        if (!modelExists || !accessoryExists)
        {
            return ServiceResult<bool>.Fail(404, "Vehicle model or accessory not found.");
        }

        var duplicateExists = await _context.VehicleAccessoryRequirements.AnyAsync(x =>
            x.ModelId == request.ModelId &&
            x.AccessoryId == request.AccessoryId &&
            (!excludeId.HasValue || x.Id != excludeId.Value));
        if (duplicateExists)
        {
            return ServiceResult<bool>.Fail(409, "Requirement already exists for this model and accessory.");
        }

        return ServiceResult<bool>.SuccessResult(true);
    }

    private async Task<ServiceResult<VehicleAccessoryRequirementDto>> GetRequirementByIdAsync(int id)
    {
        var item = await _context.VehicleAccessoryRequirements.AsNoTracking()
            .Include(x => x.Model)
            .Include(x => x.Accessory)
            .Where(x => x.Id == id)
            .Select(x => new VehicleAccessoryRequirementDto
            {
                Id = x.Id,
                ModelId = x.ModelId,
                ModelName = x.Model.ModelName,
                AccessoryId = x.AccessoryId,
                AccessoryCode = x.Accessory.Code,
                AccessoryName = x.Accessory.Name,
                ImageUrl = x.Accessory.ImageUrl,
                RequiredQuantity = x.RequiredQuantity,
                IsMandatory = x.IsMandatory,
                Notes = x.Notes
            })
            .FirstOrDefaultAsync();

        return item == null
            ? ServiceResult<VehicleAccessoryRequirementDto>.Fail(404, "Vehicle accessory requirement not found.")
            : ServiceResult<VehicleAccessoryRequirementDto>.SuccessResult(item);
    }

    private async Task<VehicleAccessoryDto> ProjectVehicleAccessoryAsync(int id)
    {
        var entity = await _context.VehicleAccessories.AsNoTracking()
            .Include(x => x.Vehicle)
            .Include(x => x.Branch)
            .Include(x => x.Accessory)
            .Include(x => x.InstalledByNavigation)
            .Include(x => x.RemovedByNavigation)
            .FirstAsync(x => x.Id == id);

        return MapVehicleAccessory(entity);
    }

    private static VehicleAccessoryRequirementDto MapRequirement(VehicleAccessoryRequirement entity)
    {
        return new VehicleAccessoryRequirementDto
        {
            Id = entity.Id,
            ModelId = entity.ModelId,
            ModelName = entity.Model.ModelName,
            AccessoryId = entity.AccessoryId,
            AccessoryCode = entity.Accessory.Code,
            AccessoryName = entity.Accessory.Name,
            ImageUrl = entity.Accessory.ImageUrl,
            RequiredQuantity = entity.RequiredQuantity,
            IsMandatory = entity.IsMandatory,
            Notes = entity.Notes
        };
    }

    private static VehicleAccessoryDto MapVehicleAccessory(VehicleAccessory entity)
    {
        return new VehicleAccessoryDto
        {
            Id = entity.Id,
            VehicleId = entity.VehicleId,
            VehicleLicensePlate = entity.Vehicle?.LicensePlate,
            BranchId = entity.BranchId,
            BranchName = entity.Branch?.Name,
            AccessoryId = entity.AccessoryId,
            AccessoryCode = entity.Accessory?.Code,
            AccessoryName = entity.Accessory?.Name,
            AccessoryType = entity.Accessory?.Type,
            SourceTransactionId = entity.SourceTransactionId,
            Quantity = entity.Quantity,
            Status = entity.Status,
            InstallDate = entity.InstallDate,
            RemoveDate = entity.RemoveDate,
            Notes = entity.Notes,
            InstalledBy = entity.InstalledBy,
            InstalledByName = entity.InstalledByNavigation?.Name,
            RemovedBy = entity.RemovedBy,
            RemovedByName = entity.RemovedByNavigation?.Name,
            CreatedAt = entity.CreatedAt,
            UpdatedAt = entity.UpdatedAt
        };
    }
}
