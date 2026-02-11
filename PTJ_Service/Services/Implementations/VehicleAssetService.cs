using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Repositories.Interfaces;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Services;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public sealed class VehicleAssetService : IVehicleAssetService
{
    private readonly IVehicleAssetRepository _repository;

    public VehicleAssetService(IVehicleAssetRepository repository)
    {
        _repository = repository;
    }

    public Task<List<VehicleAssetDto>> GetVehiclesAsync(int? branchId, string? status, bool includeDeleted)
    {
        return _repository.GetVehiclesAsync(branchId, status, includeDeleted);
    }

    public Task<VehicleAssetDto?> GetVehicleByIdAsync(int id)
    {
        return _repository.GetVehicleByIdAsync(id);
    }

    public async Task<ServiceResult<VehicleAssetDto>> CreateVehicleAsync(VehicleCreateRequestDto request)
    {
        if (request == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "Request body is required.");
        }

        var licensePlate = request.LicensePlate?.Trim();
        if (string.IsNullOrWhiteSpace(licensePlate))
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "LicensePlate is required.");
        }

        if (request.ModelId is null || request.ModelId <= 0)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "ModelId is required.");
        }

        if (request.OriginalCost.HasValue && request.OriginalCost.Value < 0)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "OriginalCost must be >= 0.");
        }

        if (request.CurrentValue.HasValue && request.CurrentValue.Value < 0)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "CurrentValue must be >= 0.");
        }

        if (request.Mileage.HasValue && request.Mileage.Value < 0)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "Mileage must be >= 0.");
        }

        var licenseExists = await _repository.LicensePlateExistsAsync(licensePlate);
        if (licenseExists)
        {
            return ServiceResult<VehicleAssetDto>.Fail(409, "LicensePlate already exists.");
        }

        var modelExists = await _repository.ModelExistsAsync(request.ModelId.Value);
        if (!modelExists)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "ModelId not found.");
        }

        if (request.CurrentBranchId.HasValue)
        {
            var branchExists = await _repository.BranchExistsAsync(request.CurrentBranchId.Value);
            if (!branchExists)
            {
                return ServiceResult<VehicleAssetDto>.Fail(400, "CurrentBranchId not found.");
            }
        }

        if (request.CurrentDriverId.HasValue)
        {
            var driverExists = await _repository.DriverExistsAsync(request.CurrentDriverId.Value);
            if (!driverExists)
            {
                return ServiceResult<VehicleAssetDto>.Fail(400, "CurrentDriverId not found.");
            }
        }

        var status = string.IsNullOrWhiteSpace(request.Status) ? "Active" : request.Status.Trim();
        var currentValue = request.CurrentValue ?? request.OriginalCost;
        var mileage = request.Mileage ?? 0m;

        var vehicle = new Vehicle
        {
            LicensePlate = licensePlate,
            ModelId = request.ModelId,
            YearManufacture = request.YearManufacture,
            PurchaseDate = request.PurchaseDate,
            OriginalCost = request.OriginalCost,
            CurrentValue = currentValue,
            Mileage = mileage,
            Status = status,
            CurrentBranchId = request.CurrentBranchId,
            CurrentDriverId = request.CurrentDriverId
        };

        var created = await _repository.AddVehicleAsync(vehicle);
        var createdDto = await _repository.GetVehicleByIdAsync(created.Id);
        if (createdDto == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(500, "Failed to load created vehicle.");
        }

        return ServiceResult<VehicleAssetDto>.SuccessResult(createdDto, 201);
    }
}
