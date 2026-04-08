using Models.DTO.Branch;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.VehicleAssets.Interfaces
{
    public interface IBranchService
    {
        Task<List<BranchResponseDto>> GetAllAsync();
        Task<BranchResponseDto?> GetByIdAsync(int id);
        Task<BranchResponseDto> CreateAsync(BranchCreateDto dto);
        Task<BranchResponseDto?> UpdateAsync(BranchUpdateDto dto);
        Task<bool> DeleteAsync(int id);
        Task<List<BranchDropdownDto>> DropdownAsync();
    }
}
