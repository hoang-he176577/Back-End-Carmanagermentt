using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Data.Repositories.VehicleAssets.Implementations
{
    public class TripLogRepository : ITripLogRepository
    {
        private readonly CarManagerContext _context;

        public TripLogRepository(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<List<TripLog>> GetAllAsync()
        {
            return await _context.TripLogs
                .OrderByDescending(x => x.StartTime)
                .ToListAsync();
        }

        public async Task<TripLog?> GetByIdAsync(int id)
        {
            return await _context.TripLogs
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> HasActiveTripAsync(int vehicleId)
        {
            return await _context.TripLogs
                .AnyAsync(x => x.VehicleId == vehicleId && x.EndTime == null);
        }

        public async Task AddAsync(TripLog trip)
        {
            await _context.TripLogs.AddAsync(trip);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
