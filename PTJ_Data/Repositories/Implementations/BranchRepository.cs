using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.Implementations
{
    public class BranchRepository : IBranchRepository
    {
        private readonly CarManagerContext _context;

        public BranchRepository(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<List<Branch>> GetAllAsync()
        {
            return await _context.Branches
                .Where(x => x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Branch?> GetByIdAsync(int id)
        {
            return await _context.Branches
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        }

        public async Task<Branch> CreateAsync(Branch branch)
        {
            branch.CreatedAt = DateTime.UtcNow;

            _context.Branches.Add(branch);
            await _context.SaveChangesAsync();

            return branch;
        }

        public async Task<Branch?> UpdateAsync(Branch branch)
        {
            var existing = await _context.Branches.FindAsync(branch.Id);

            if (existing == null) return null;

            existing.Name = branch.Name;
            existing.Address = branch.Address;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var branch = await _context.Branches.FindAsync(id);

            if (branch == null) return false;

            branch.DeletedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            return true;
        }
    }
}
