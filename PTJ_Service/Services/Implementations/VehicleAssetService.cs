using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Repositories.Interfaces;
using Data.Repositories.VehicleAssets.Interfaces;
using Models.Common;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Services;
using Service.Services.Common;
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

    

    public async Task<ServiceResult<VehicleAssetDto>> CreateAssetAsync(VehicleAssetCreateRequestDto request, int accountantUserId)
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

        var vin = request.Vin?.Trim();
        var engineNumber = request.EngineNumber?.Trim();
        var chassisNumber = request.ChassisNumber?.Trim();

        if (string.IsNullOrWhiteSpace(vin))
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "Vin is required.");
        }

        if (string.IsNullOrWhiteSpace(engineNumber))
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "EngineNumber is required.");
        }

        if (string.IsNullOrWhiteSpace(chassisNumber))
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "ChassisNumber is required.");
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

        if (request.RegistrationCost.HasValue && request.RegistrationCost.Value < 0)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "RegistrationCost must be >= 0.");
        }

        if (request.InsuranceCost.HasValue && request.InsuranceCost.Value < 0)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "InsuranceCost must be >= 0.");
        }

        if (request.RegistrationIssueDate.HasValue && request.RegistrationExpiryDate.HasValue &&
            request.RegistrationExpiryDate.Value < request.RegistrationIssueDate.Value)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "RegistrationExpiryDate must be >= RegistrationIssueDate.");
        }

        if (request.InsuranceStartDate.HasValue && request.InsuranceExpiryDate.HasValue &&
            request.InsuranceExpiryDate.Value < request.InsuranceStartDate.Value)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "InsuranceExpiryDate must be >= InsuranceStartDate.");
        }

        // Uniqueness checks
        if (await _repository.LicensePlateExistsAsync(licensePlate))
        {
            return ServiceResult<VehicleAssetDto>.Fail(409, "LicensePlate already exists.");
        }

        if (await _repository.VinExistsAsync(vin))
        {
            return ServiceResult<VehicleAssetDto>.Fail(409, "Vin already exists.");
        }

        if (await _repository.EngineNumberExistsAsync(engineNumber))
        {
            return ServiceResult<VehicleAssetDto>.Fail(409, "EngineNumber already exists.");
        }

        if (await _repository.ChassisNumberExistsAsync(chassisNumber))
        {
            return ServiceResult<VehicleAssetDto>.Fail(409, "ChassisNumber already exists.");
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

        var status = string.IsNullOrWhiteSpace(request.Status)
            ? VehicleStatus.Available
            : request.Status.Trim();

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
            CurrentDriverId = request.CurrentDriverId,
            Vin = vin,
            EngineNumber = engineNumber,
            ChassisNumber = chassisNumber,
            Color = string.IsNullOrWhiteSpace(request.Color) ? null : request.Color.Trim(),
            SeatCount = request.SeatCount,
            FuelType = string.IsNullOrWhiteSpace(request.FuelType) ? null : request.FuelType.Trim(),
            WarrantyExpiryDate = request.WarrantyExpiryDate
        };

        var createdVehicle = await _repository.AddVehicleAsync(vehicle);

        // Optional registration record
        if (!string.IsNullOrWhiteSpace(request.RegistrationNumber) ||
            request.RegistrationIssueDate.HasValue ||
            request.RegistrationExpiryDate.HasValue ||
            request.RegistrationCost.HasValue ||
            !string.IsNullOrWhiteSpace(request.RegistrationAuthority) ||
            !string.IsNullOrWhiteSpace(request.RegistrationNotes))
        {
            var registration = new RegistrationRecord
            {
                VehicleId = createdVehicle.Id,
                RegistrationNumber = string.IsNullOrWhiteSpace(request.RegistrationNumber)
                    ? null
                    : request.RegistrationNumber.Trim(),
                Authority = string.IsNullOrWhiteSpace(request.RegistrationAuthority)
                    ? null
                    : request.RegistrationAuthority.Trim(),
                IssueDate = request.RegistrationIssueDate,
                ExpiryDate = request.RegistrationExpiryDate,
                Cost = request.RegistrationCost,
                Notes = request.RegistrationNotes
            };

            await _repository.AddRegistrationRecordAsync(registration);
        }

        // Optional insurance record
        if (!string.IsNullOrWhiteSpace(request.InsurancePolicyNumber) ||
            !string.IsNullOrWhiteSpace(request.InsuranceProvider) ||
            request.InsuranceStartDate.HasValue ||
            request.InsuranceExpiryDate.HasValue ||
            request.InsuranceCost.HasValue ||
            !string.IsNullOrWhiteSpace(request.InsuranceCoverageDetails))
        {
            var insurance = new InsuranceRecord
            {
                VehicleId = createdVehicle.Id,
                PolicyNumber = string.IsNullOrWhiteSpace(request.InsurancePolicyNumber)
                    ? null
                    : request.InsurancePolicyNumber.Trim(),
                Provider = string.IsNullOrWhiteSpace(request.InsuranceProvider)
                    ? null
                    : request.InsuranceProvider.Trim(),
                StartDate = request.InsuranceStartDate,
                ExpiryDate = request.InsuranceExpiryDate,
                Cost = request.InsuranceCost,
                CoverageDetails = request.InsuranceCoverageDetails
            };

            await _repository.AddInsuranceRecordAsync(insurance);
        }

        // Asset change log cho Accountant
        var changeLog = new AssetChangeLog
        {
            VehicleId = createdVehicle.Id,
            ChangeType = "CREATE",
            ChangeDate = DateOnly.FromDateTime(DateTime.UtcNow),
            AmountChange = request.OriginalCost,
            Reason = string.IsNullOrWhiteSpace(request.Notes) ? "Initial asset creation." : request.Notes,
            AccountantId = accountantUserId
        };

        await _repository.AddAssetChangeLogAsync(changeLog);

        var createdDto = await _repository.GetVehicleByIdAsync(createdVehicle.Id);
        if (createdDto == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(500, "Failed to load created asset.");
        }

        return ServiceResult<VehicleAssetDto>.SuccessResult(createdDto, 201);
    }

    public async Task<ServiceResult<VehicleAssetDto>> AssignVehicleAsync(int vehicleId, VehicleAssignRequestDto request, int performedByUserId)
    {
        if (request == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "Request body is required.");
        }

        if (request.DriverId <= 0)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "DriverId is required.");
        }

        var vehicle = await _repository.GetVehicleEntityByIdAsync(vehicleId);
        if (vehicle == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(404, "Vehicle not found.");
        }

        if (!string.Equals(vehicle.Status, VehicleStatus.Available, StringComparison.OrdinalIgnoreCase) &&
            vehicle.CurrentDriverId != null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(409, "Vehicle is not available for assignment.");
        }

        var driver = await _repository.GetDriverByIdAsync(request.DriverId);
        if (driver == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "Driver not found.");
        }

        vehicle.CurrentDriverId = request.DriverId;
        vehicle.Status = VehicleStatus.Assigned;
        vehicle.UpdatedAt = DateTime.UtcNow;

        var assignDate = request.AssignDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        var history = new VehicleDriverHistory
        {
            VehicleId = vehicle.Id,
            DriverId = request.DriverId,
            AssignDate = assignDate,
            Notes = request.Notes
        };

        await _repository.AddDriverHistoryAsync(history);

        var dto = await _repository.GetVehicleByIdAsync(vehicle.Id);
        if (dto == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(500, "Failed to load assigned vehicle.");
        }

        return ServiceResult<VehicleAssetDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<VehicleAssetDto>> UnassignVehicleAsync(int vehicleId, VehicleUnassignRequestDto request, int performedByUserId)
    {
        var vehicle = await _repository.GetVehicleEntityByIdAsync(vehicleId);
        if (vehicle == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(404, "Vehicle not found.");
        }

        if (!string.Equals(vehicle.Status, VehicleStatus.Assigned, StringComparison.OrdinalIgnoreCase) ||
            vehicle.CurrentDriverId == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(400, "Vehicle is not currently assigned.");
        }

        var history = await _repository.GetLatestActiveDriverHistoryAsync(vehicle.Id);
        var unassignDate = request.UnassignDate ?? DateOnly.FromDateTime(DateTime.UtcNow);

        vehicle.CurrentDriverId = null;
        vehicle.Status = VehicleStatus.Available;
        vehicle.UpdatedAt = DateTime.UtcNow;

        if (history != null)
        {
            history.UnassignDate = unassignDate;
            history.Notes = string.IsNullOrWhiteSpace(request.Notes) ? history.Notes : request.Notes;
        }

        await _repository.SaveChangesAsync();

        var dto = await _repository.GetVehicleByIdAsync(vehicle.Id);
        if (dto == null)
        {
            return ServiceResult<VehicleAssetDto>.Fail(500, "Failed to load unassigned vehicle.");
        }

        return ServiceResult<VehicleAssetDto>.SuccessResult(dto);
    }
}

