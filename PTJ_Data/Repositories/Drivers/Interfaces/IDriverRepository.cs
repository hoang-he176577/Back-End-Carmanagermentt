using Models.DTO.Drivers;
using Models.Models;

namespace Data.Repositories.Drivers.Interfaces;

public interface IDriverRepository
{
    Task<List<DriverResponseDto>> GetAllAsync(int? branchId);
    Task<DriverResponseDto?> GetByIdAsync(int id);
    Task<Driver> CreateAsync(Driver driver);
    Task UpdateAsync(Driver driver);
    Task<bool> SoftDeleteAsync(int id);
    Task<List<DriverDropdownDto>> GetAvailableByBranchAsync(int branchId);
    Task<List<DriverDropdownDto>> GetDropdownAsync(int? branchId);
    Task<bool> LicenseNumberExistsAsync(string licenseNumber, int? excludeId);
    Task<Driver?> GetEntityByIdAsync(int id);
    Task<int?> GetUserBranchIdAsync(int userId);
    Task<bool> BranchExistsAsync(int branchId);
}
