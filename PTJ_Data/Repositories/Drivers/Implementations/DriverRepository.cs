using System.Linq.Expressions;
using Data.Repositories.Drivers.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Drivers;
using Models.Models;

namespace Data.Repositories.Drivers.Implementations;

public sealed class DriverRepository : IDriverRepository
{
    private readonly CarManagerContext _context;

    private static readonly Expression<Func<Driver, DriverResponseDto>> DriverSelector = d => new DriverResponseDto
    {
        Id = d.Id,
        Name = d.Name,
        LicenseNumber = d.LicenseNumber,
        Phone = d.Phone,
        Email = d.Email,
        HireDate = d.HireDate,
        Status = d.Status,
        BranchId = d.BranchId,
        BranchName = d.Branch != null ? d.Branch.Name : null,
        CurrentVehicleId = d.Vehicles.Where(v => v.DeletedAt == null).Select(v => (int?)v.Id).FirstOrDefault(),
        CurrentVehicleLicensePlate = d.Vehicles.Where(v => v.DeletedAt == null).Select(v => v.LicensePlate).FirstOrDefault(),
        CurrentVehicleModelName = d.Vehicles.Where(v => v.DeletedAt == null && v.Model != null).Select(v => v.Model!.ModelName).FirstOrDefault(),
        CreatedAt = d.CreatedAt,
        UpdatedAt = d.UpdatedAt
    };

    private static readonly Expression<Func<Driver, DriverDropdownDto>> DropdownSelector = d => new DriverDropdownDto
    {
        Id = d.Id,
        Name = d.Name,
        LicenseNumber = d.LicenseNumber,
        BranchId = d.BranchId,
        BranchName = d.Branch != null ? d.Branch.Name : null,
        IsAssigned = d.Vehicles.Any(v => v.DeletedAt == null)
    };

    public DriverRepository(CarManagerContext context)
    {
        _context = context;
    }

    public Task<List<DriverResponseDto>> GetAllAsync(int? branchId)
    {
        var query = _context.Drivers.AsNoTracking().Where(d => d.DeletedAt == null);
        if (branchId.HasValue)
        {
            query = query.Where(d => d.BranchId == branchId.Value);
        }

        return query.OrderBy(d => d.Id).Select(DriverSelector).ToListAsync();
    }

    public Task<DriverResponseDto?> GetByIdAsync(int id)
    {
        return _context.Drivers.AsNoTracking()
            .Where(d => d.Id == id && d.DeletedAt == null)
            .Select(DriverSelector)
            .FirstOrDefaultAsync();
    }

    public async Task<Driver> CreateAsync(Driver driver)
    {
        _context.Drivers.Add(driver);
        await _context.SaveChangesAsync();
        return driver;
    }

    public Task UpdateAsync(Driver driver)
    {
        _context.Drivers.Update(driver);
        return _context.SaveChangesAsync();
    }

    public async Task<bool> SoftDeleteAsync(int id)
    {
        var entity = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);
        if (entity == null)
        {
            return false;
        }

        entity.DeletedAt = DateTime.Now;
        entity.UpdatedAt = DateTime.Now;
        await _context.SaveChangesAsync();
        return true;
    }

    public Task<List<DriverDropdownDto>> GetAvailableByBranchAsync(int branchId)
    {
        return _context.Drivers.AsNoTracking()
            .Where(d => d.DeletedAt == null
                && d.BranchId == branchId
                && !d.Vehicles.Any(v => v.DeletedAt == null))
            .OrderBy(d => d.Name)
            .Select(DropdownSelector)
            .ToListAsync();
    }

    public Task<List<DriverDropdownDto>> GetDropdownAsync(int? branchId)
    {
        var query = _context.Drivers.AsNoTracking().Where(d => d.DeletedAt == null);
        if (branchId.HasValue)
        {
            query = query.Where(d => d.BranchId == branchId.Value);
        }

        return query.OrderBy(d => d.Name).Select(DropdownSelector).ToListAsync();
    }

    public Task<bool> LicenseNumberExistsAsync(string licenseNumber, int? excludeId)
    {
        var query = _context.Drivers.AsNoTracking()
            .Where(d => d.DeletedAt == null && d.LicenseNumber == licenseNumber);
        if (excludeId.HasValue)
        {
            query = query.Where(d => d.Id != excludeId.Value);
        }

        return query.AnyAsync();
    }

    public Task<Driver?> GetEntityByIdAsync(int id)
    {
        return _context.Drivers.FirstOrDefaultAsync(d => d.Id == id && d.DeletedAt == null);
    }

    public Task<int?> GetUserBranchIdAsync(int userId)
    {
        return _context.Users.AsNoTracking()
            .Where(u => u.Id == userId && u.DeletedAt == null)
            .Select(u => u.BranchId)
            .FirstOrDefaultAsync();
    }

    public Task<bool> BranchExistsAsync(int branchId)
    {
        return _context.Branches.AsNoTracking()
            .AnyAsync(b => b.Id == branchId && b.DeletedAt == null);
    }
}
