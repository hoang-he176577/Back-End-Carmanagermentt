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
            await _context.TripLogs.AddAsync(trip);
            await _context.SaveChangesAsync();
            return trip;
        }

        public async Task<TripLog?> GetRunningTripByVehicleIdAsync(int vehicleId)
        {
            return await _context.TripLogs
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.VehicleId == vehicleId && t.EndTime == null);
        }

        public async Task<TripLog?> GetLastCompletedTripByVehicleIdAsync(int vehicleId)
        {
            return await _context.TripLogs
                .Where(t => t.VehicleId == vehicleId && t.EndTime != null)
                .OrderByDescending(t => t.EndTime)
                .FirstOrDefaultAsync();
        }

        public async Task<TripLog?> GetLastCompletedTripByDriverIdAsync(int driverId)
        {
            return await _context.TripLogs
                .Where(t => t.DriverId == driverId && t.EndTime != null)
                .OrderByDescending(t => t.EndTime)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TripLog>> GetLastCompletedTripsByVehicleIdsAsync(List<int> vehicleIds)
        {
            if (vehicleIds == null || vehicleIds.Count == 0)
            {
                return new List<TripLog>();
            }

            return await _context.TripLogs
                .Where(t => vehicleIds.Contains(t.VehicleId) && t.EndTime != null)
                .GroupBy(t => t.VehicleId)
                .Select(g => g.OrderByDescending(x => x.EndTime).First())
                .ToListAsync();
        }

        public async Task<TripLog?> GetByIdAsync(int tripId)
        {
            return await _context.TripLogs
                .FirstOrDefaultAsync(t => t.Id == tripId);
        }

        public async Task UpdateAsync(TripLog trip)
        {
            _context.TripLogs.Update(trip);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TripLog>> GetTripHistoryByVehicleAsync(int? vehicleId)
        {
            if(vehicleId == null)
            {
                return await _context.TripLogs
                .Include(v => v.Driver)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
            }
            return await _context.TripLogs
                .Include(v => v.Driver)
                .Where(t => t.VehicleId == vehicleId)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<TripLog?> GetByIdAsync(int id)
        {
            return await _context.TripLog
                .FirstOrDefaultAsync(x => x.Id == id);
        }

        public async Task<bool> HasActiveTripAsync(int vehicleId)
        {
            return await _context.TripLog
                .AnyAsync(x => x.VehicleId == vehicleId && x.EndTime == null);
        }

        public async Task AddAsync(TripLog trip)
        {
            await _context.TripLog.AddAsync(trip);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
