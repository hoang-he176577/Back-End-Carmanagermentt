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

    public class PurchaseProposalRepository : IPurchaseProposalRepository
    {
        private readonly CarManagerContext _context;

        public PurchaseProposalRepository(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<PurchaseProposal?> GetByIdAsync(int id)
        {
            return await _context.PurchaseProposals
                .Include(x => x.BulkPurchaseDetails)
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<List<PurchaseProposal>> GetAllAsync()
        {
            return await _context.PurchaseProposals
                .Include(x => x.BulkPurchaseDetails)
                .ToListAsync();
        }

        public async Task AddAsync(PurchaseProposal proposal)
        {
            await _context.PurchaseProposals.AddAsync(proposal);
        }

        public void Update(PurchaseProposal proposal)
        {
            _context.PurchaseProposals.Update(proposal);
        }

        public void Delete(PurchaseProposal proposal)
        {
            _context.PurchaseProposals.Remove(proposal);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
