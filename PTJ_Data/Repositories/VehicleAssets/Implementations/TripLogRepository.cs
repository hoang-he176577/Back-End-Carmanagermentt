using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Models;
using System.Collections.Generic;
using System.Linq;
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
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<TripLog?> GetByIdAsync(int id)
        {
            return await _context.TripLogs
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .FirstOrDefaultAsync(t => t.Id == id);
        }

        public async Task<TripLog?> GetRunningTripByVehicleIdAsync(int vehicleId)
        {
            return await _context.TripLogs
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
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
                .Where(t => vehicleIds.Contains(t.VehicleId ?? 0) && t.EndTime != null)
                .GroupBy(t => t.VehicleId)
                .Select(g => g.OrderByDescending(x => x.EndTime).First())
                .ToListAsync();
        }

        public async Task<List<TripLog>> GetTripHistoryByVehicleAsync(int vehicleId)
        {
            return await _context.TripLogs
                .Include(t => t.Driver)
                .Where(t => t.VehicleId == vehicleId)
                .OrderByDescending(t => t.StartTime)
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
        {
            return await _context.Vehicles
                .Include(v => v.CurrentBranch)
                .Include(v => v.CurrentDriver)
                .FirstOrDefaultAsync(v => v.Id == vehicleId);
        }

        public async Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId)
        {
            return await _context.Vehicles
                .Include(v => v.CurrentBranch)
                .Include(v => v.CurrentDriver)
                .Where(v => v.CurrentBranchId == branchId)
                .ToListAsync();
        }

        public async Task<List<TripLog>> GetRunningTripsByBranchAsync(int branchId)
        {
            return await _context.TripLogs
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .Where(t => t.EndTime == null && t.Vehicle != null && t.Vehicle.CurrentBranchId == branchId)
                .ToListAsync();
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

        public Task UpdateAsync(TripLog trip)
        {
            _context.TripLogs.Update(trip);
            return Task.CompletedTask;
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
