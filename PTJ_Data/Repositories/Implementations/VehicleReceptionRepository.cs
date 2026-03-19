using Data.Repositories.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Data.Repositories.Implementations
{
    public class VehicleReceptionRepository : IVehicleReceptionRepository
    {
        private readonly CarManagerContext _context;

        public VehicleReceptionRepository(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<VehicleReceptionRecord?> GetByIdAsync(int id)
        {
            return await _context.VehicleReceptionRecords
                .Include(x => x.PurchaseProposal)
                .Include(x => x.Branch)
                .Include(x => x.Operator)
                .AsNoTracking()
                .FirstOrDefaultAsync(x => x.Id == id && x.DeletedAt == null);
        }

        public async Task<List<VehicleReceptionRecord>> GetByProposalIdAsync(int proposalId)
        {
            return await _context.VehicleReceptionRecords
                .Include(x => x.Branch)
                .Include(x => x.Operator)
                .AsNoTracking()
                .Where(x => x.PurchaseProposalId == proposalId && x.DeletedAt == null)
                .ToListAsync();
        }

        public async Task<List<VehicleReceptionRecord>> GetByBranchIdAsync(int branchId)
        {
            return await _context.VehicleReceptionRecords
                .Include(x => x.PurchaseProposal)
                .Include(x => x.Operator)
                .AsNoTracking()
                .Where(x => x.BranchId == branchId && x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VehicleReceptionRecord>> GetPendingByBranchAsync(int branchId)
        {
            return await _context.VehicleReceptionRecords
                .Include(x => x.PurchaseProposal)
                .Include(x => x.Operator)
                .AsNoTracking()
                .Where(x => x.BranchId == branchId && x.Status == "Pending" && x.DeletedAt == null)
                .OrderByDescending(x => x.RequestedDate)
                .ToListAsync();
        }

        public async Task<List<VehicleReceptionRecord>> GetAllAsync()
        {
            return await _context.VehicleReceptionRecords
                .Include(x => x.PurchaseProposal)
                .Include(x => x.Branch)
                .Include(x => x.Operator)
                .AsNoTracking()
                .Where(x => x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<VehicleReceptionRecord>> GetByStatusAsync(string status)
        {
            return await _context.VehicleReceptionRecords
                .Include(x => x.PurchaseProposal)
                .Include(x => x.Branch)
                .Include(x => x.Operator)
                .AsNoTracking()
                .Where(x => x.Status == status && x.DeletedAt == null)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<VehicleReceptionRecord> AddAsync(VehicleReceptionRecord record)
        {
            await _context.VehicleReceptionRecords.AddAsync(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<VehicleReceptionRecord> UpdateAsync(VehicleReceptionRecord record)
        {
            record.UpdatedAt = DateTime.Now;
            _context.VehicleReceptionRecords.Update(record);
            await _context.SaveChangesAsync();
            return record;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var record = await _context.VehicleReceptionRecords.FindAsync(id);
            if (record == null)
                return false;

            record.DeletedAt = DateTime.Now;
            _context.VehicleReceptionRecords.Update(record);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            return await _context.VehicleReceptionRecords
                .AnyAsync(x => x.Id == id && x.DeletedAt == null);
        }
    }
}
