using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Vehicles;
using Models.Models;

namespace Data.Repositories.VehicleAssets.Implementations;

public sealed class VehicleAssetRepository : IVehicleAssetRepository
{
    private readonly CarManagerContext _context;

    private static readonly Expression<Func<Vehicle, VehicleAssetDto>> VehicleAssetSelector = vehicle => new VehicleAssetDto
    {
        Id = vehicle.Id,
        LicensePlate = vehicle.LicensePlate,
        ModelId = vehicle.ModelId,
        Manufacturer = vehicle.Model != null ? vehicle.Model.Manufacturer : null,
        ModelName = vehicle.Model != null ? vehicle.Model.ModelName : null,
        YearManufacture = vehicle.YearManufacture,
        PurchaseDate = vehicle.PurchaseDate,
        OriginalCost = vehicle.OriginalCost,
        CurrentValue = vehicle.CurrentValue,
        Mileage = vehicle.Mileage,
        Status = vehicle.Status,
        CurrentBranchId = vehicle.CurrentBranchId,
        CurrentBranchName = vehicle.CurrentBranch != null ? vehicle.CurrentBranch.Name : null,
        CurrentDriverId = vehicle.CurrentDriverId,
        CurrentDriverName = vehicle.CurrentDriver != null ? vehicle.CurrentDriver.Name : null
    };

    public VehicleAssetRepository(CarManagerContext context)
    {
        _context = context;
    }

    public async Task<List<VehicleAssetDto>> GetVehiclesAsync(int? branchId, string? status, bool includeDeleted)
    {
        var query = _context.Vehicles.AsNoTracking();

        if (!includeDeleted)
        {
            query = query.Where(v => v.DeletedAt == null);
        }

        if (branchId.HasValue)
        {
            query = query.Where(v => v.CurrentBranchId == branchId.Value);
        }

        if (!string.IsNullOrWhiteSpace(status))
        {
            var trimmedStatus = status.Trim();
            query = query.Where(v => v.Status == trimmedStatus);
        }

        return await query
            .OrderBy(v => v.Id)
            .Select(VehicleAssetSelector)
            .ToListAsync();
    }

    public async Task<VehicleAssetDto?> GetVehicleByIdAsync(int id)
    {
        return await _context.Vehicles.AsNoTracking()
            .Where(v => v.Id == id && v.DeletedAt == null)
            .Select(VehicleAssetSelector)
            .FirstOrDefaultAsync();
    }

    public Task<Vehicle?> GetVehicleEntityByIdAsync(int id)
    {
        return _context.Vehicles.FirstOrDefaultAsync(v => v.Id == id && v.DeletedAt == null);
    }

    public Task<bool> LicensePlateExistsAsync(string licensePlate)
    {
        return _context.Vehicles.AsNoTracking()
            .AnyAsync(v => v.LicensePlate == licensePlate);
    }

    public Task<bool> VinExistsAsync(string vin)
    {
        return _context.Vehicles.AsNoTracking()
            .AnyAsync(v => v.Vin == vin);
    }

    public Task<bool> EngineNumberExistsAsync(string engineNumber)
    {
        return _context.Vehicles.AsNoTracking()
            .AnyAsync(v => v.EngineNumber == engineNumber);
    }

    public Task<bool> ChassisNumberExistsAsync(string chassisNumber)
    {
        return _context.Vehicles.AsNoTracking()
            .AnyAsync(v => v.ChassisNumber == chassisNumber);
    }

    public Task<bool> LicensePlateExistsForOtherVehicleAsync(string licensePlate, int vehicleId)
    {
        return _context.Vehicles.AsNoTracking()
            .AnyAsync(v => v.Id != vehicleId && v.LicensePlate == licensePlate);
    }

    public Task<bool> ModelExistsAsync(int modelId)
    {
        return _context.VehicleModels.AsNoTracking()
            .AnyAsync(m => m.Id == modelId && m.DeletedAt == null);
    }

    public Task<bool> BranchExistsAsync(int branchId)
    {
        return _context.Branches.AsNoTracking()
            .AnyAsync(b => b.Id == branchId && b.DeletedAt == null);
    }

    public Task<bool> DriverExistsAsync(int driverId)
    {
        return _context.Drivers.AsNoTracking()
            .AnyAsync(d => d.Id == driverId && d.DeletedAt == null);
    }

    public Task<int?> GetUserBranchIdAsync(int userId)
    {
        return _context.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => u.BranchId)
            .FirstOrDefaultAsync();
    }

    public async Task<Vehicle> AddVehicleAsync(Vehicle vehicle)
    {
        _context.Vehicles.Add(vehicle);
        await _context.SaveChangesAsync();
        return vehicle;
    }

    public async Task AddRegistrationRecordAsync(RegistrationRecord record)
    {
        _context.RegistrationRecords.Add(record);
        await _context.SaveChangesAsync();
    }

    public async Task AddInsuranceRecordAsync(InsuranceRecord record)
    {
        _context.InsuranceRecords.Add(record);
        await _context.SaveChangesAsync();
    }

    public async Task AddAssetChangeLogAsync(AssetChangeLog log)
    {
        _context.AssetChangeLogs.Add(log);
        await _context.SaveChangesAsync();
    }

    public Task<Driver?> GetDriverByIdAsync(int id)
    {
        return _context.Drivers
            .FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);
    }

    public Task<VehicleDriverHistory?> GetLatestActiveDriverHistoryAsync(int vehicleId)
    {
        return _context.VehicleDriverHistories
            .Where(h => h.VehicleId == vehicleId && h.UnassignDate == null)
            .OrderByDescending(h => h.AssignDate)
            .FirstOrDefaultAsync();
    }

    public async Task AddDriverHistoryAsync(VehicleDriverHistory history)
    {
        _context.VehicleDriverHistories.Add(history);
        await _context.SaveChangesAsync();
    }

    public Task SaveChangesAsync()
    {
        return _context.SaveChangesAsync();
    }

    // ===== DROPDOWN DATA METHODS =====

    public async Task<List<VehicleModelDto>> GetVehicleModelsAsync()
    {
        return await _context.VehicleModels
            .AsNoTracking()
            .Where(m => m.DeletedAt == null)
            .OrderBy(m => m.Manufacturer)
            .ThenBy(m => m.ModelName)
            .Select(m => new VehicleModelDto
            {
                Id = m.Id,
                Manufacturer = m.Manufacturer ?? "Unknown",
                ModelName = m.ModelName ?? "Unknown",
                Seats = m.Seats,
                EngineType = m.EngineType,
                DefaultPrice = m.DefaultPrice
            })
            .ToListAsync();
    }

    public async Task<List<BranchDto>> GetBranchesAsync()
    {
        return await _context.Branches
            .AsNoTracking()
            .Where(b => b.DeletedAt == null)
            .OrderBy(b => b.Name)
            .Select(b => new BranchDto
            {
                Id = b.Id,
                Name = b.Name ?? "Unknown",
                Address = b.Address
            })
            .ToListAsync();
    }

    public async Task<List<DriverDto>> GetDriversAsync()
    {
        return await _context.Drivers
            .AsNoTracking()
            .Where(d => d.DeletedAt == null && d.Status == "Active")
            .OrderBy(d => d.Name)
            .Select(d => new DriverDto
            {
                Id = d.Id,
                Name = d.Name ?? "Unknown",
                LicenseNumber = d.LicenseNumber,
                Phone = d.Phone
            })
            .ToListAsync();
    }
}

