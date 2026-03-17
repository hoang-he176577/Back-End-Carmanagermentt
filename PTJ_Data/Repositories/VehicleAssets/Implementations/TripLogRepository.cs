using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.PurchaseProposal;
using Models.Models;
using NHibernate.Loader.Custom;
using System;

namespace Data.Repositories.VehicleAssets.Implementations
{
    public class TripLogRepository : ITripLogRepository
    {
        private readonly CarManagerContext _context;

        public TripLogRepository(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<TripLog> CreateAsync(TripLog trip)
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

        public async Task<List<Vehicle>> GetVehiclesDropAsync()
        {
            return await _context.Vehicles
                .Include(v => v.Model)
                .Where(v => v.DeletedAt == null)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<UserBasicDto?> GetDriverByVehicleIdAsync(int vehicleId)
        {
            return await _context.Vehicles
        .Where(v => v.Id == vehicleId && v.CurrentDriver != null)
        .Select(v => new UserBasicDto
        {
            Id = v.CurrentDriver!.Id,
            Name = v.CurrentDriver.Name!
        })
        .AsNoTracking()
        .FirstOrDefaultAsync();
        }

        public async Task<Vehicle?> GetVehicleByIdAsync(int vehicleId)
        {
            return await _context.Vehicles
                .Include(v => v.CurrentBranch)
                .Include(v => v.CurrentDriver)
                .FirstOrDefaultAsync(v => v.Id == vehicleId && v.DeletedAt == null);
        }

        public async Task<List<Vehicle>> GetVehiclesByBranchAsync(int branchId)
        {
            return await _context.Vehicles
                .Include(v => v.CurrentBranch)
                .Include(v => v.CurrentDriver)
                .Where(v => v.DeletedAt == null && v.CurrentBranchId == branchId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<TripLog>> GetRunningTripsByBranchAsync(int branchId)
        {
            return await _context.TripLogs
                .Include(t => t.Vehicle)
                .Include(t => t.Driver)
                .Where(t => t.EndTime == null && t.Vehicle.CurrentBranchId == branchId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateVehicleStatusAsync(int vehicleId, string status)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null)
            {
                return;
            }

            vehicle.Status = status;
            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
