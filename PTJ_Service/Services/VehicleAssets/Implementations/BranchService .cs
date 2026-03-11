using Data.Repositories.Interfaces;
using Models.DTO.Branch;
using Models.Models;
using Service.Services.VehicleAssets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.VehicleAssets.Implementations
{
    public class BranchService : IBranchService
    {
        private readonly IBranchRepository _repository;

        public BranchService(IBranchRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<BranchResponseDto>> GetAllAsync()
        {
            var branches = await _repository.GetAllAsync();

            return branches.Select(x => new BranchResponseDto
            {
                Id = x.Id,
                Name = x.Name,
                Address = x.Address,
                CreatedAt = x.CreatedAt,
                UpdatedAt = x.UpdatedAt
            }).ToList();
        }

        public async Task<BranchResponseDto?> GetByIdAsync(int id)
        {
            var branch = await _repository.GetByIdAsync(id);

            if (branch == null) return null;

            return new BranchResponseDto
            {
                Id = branch.Id,
                Name = branch.Name,
                Address = branch.Address,
                CreatedAt = branch.CreatedAt,
                UpdatedAt = branch.UpdatedAt
            };
        }

        public async Task<BranchResponseDto> CreateAsync(BranchCreateDto dto)
        {
            var branch = new Branch
            {
                Name = dto.Name,
                Address = dto.Address
            };

            var result = await _repository.CreateAsync(branch);

            return new BranchResponseDto
            {
                Id = result.Id,
                Name = result.Name,
                Address = result.Address,
                CreatedAt = result.CreatedAt
            };
        }

        public async Task<BranchResponseDto?> UpdateAsync(BranchUpdateDto dto)
        {
            var branch = new Branch
            {
                Id = dto.Id,
                Name = dto.Name,
                Address = dto.Address
            };

            var result = await _repository.UpdateAsync(branch);

            if (result == null) return null;

            return new BranchResponseDto
            {
                Id = result.Id,
                Name = result.Name,
                Address = result.Address,
                UpdatedAt = result.UpdatedAt
            };
        }

        public async Task<bool> DeleteAsync(int id)
        {
            return await _repository.DeleteAsync(id);
        }

        public async Task<List<BranchDropdownDto>> DropdownAsync()
        {
            var branches = await _repository.GetAllAsync();

            return branches.Select(x => new BranchDropdownDto
            {
                Value = x.Id,
                Label = x.Name
            }).ToList();
        }
    }
}
