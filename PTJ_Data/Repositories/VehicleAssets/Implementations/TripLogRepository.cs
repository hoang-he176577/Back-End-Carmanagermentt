using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.PurchaseProposal;
using Models.Models;

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
                .Include(t => t.TransferPlan)
                    .ThenInclude(tp => tp.FromBranch)
                .Include(t => t.TransferPlan)
                    .ThenInclude(tp => tp.ToBranch)
                .FirstOrDefaultAsync(t => t.Id == tripId);
        }

        public async Task UpdateAsync(TripLog trip)
        {
            _context.TripLogs.Update(trip);
            await _context.SaveChangesAsync();
        }

        public async Task<List<TripLog>> GetTripHistoryByVehicleAsync(int? vehicleId)
        {
            var query = _context.TripLogs
                .Include(t => t.Driver)
                .Include(t => t.Vehicle)
                .Include(t => t.TransferPlan)
                    .ThenInclude(tp => tp.FromBranch)
                .Include(t => t.TransferPlan)
                    .ThenInclude(tp => tp.ToBranch)
                .AsQueryable();

            if (vehicleId != null)
            {
                query = query.Where(t => t.VehicleId == vehicleId);
            }

            return await query
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
                .Include(t => t.TransferPlan)
                    .ThenInclude(tp => tp.FromBranch)
                .Include(t => t.TransferPlan)
                    .ThenInclude(tp => tp.ToBranch)
                .Where(t => t.EndTime == null && t.Vehicle.CurrentBranchId == branchId)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateVehicleStatusAsync(int vehicleId, string status)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null) return;

            vehicle.Status = status;
            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        // ── Transfer plan integration ──

        public async Task<TransferPlan?> GetTransferPlanWithDetailsAsync(int transferPlanId)
        {
            return await _context.TransferPlans
                .Include(tp => tp.FromBranch)
                .Include(tp => tp.ToBranch)
                .Include(tp => tp.Vehicle)
                    .ThenInclude(v => v!.CurrentDriver)
                .FirstOrDefaultAsync(tp => tp.Id == transferPlanId && tp.DeletedAt == null);
        }

        public async Task UpdateTransferPlanAsync(TransferPlan plan)
        {
            _context.TransferPlans.Update(plan);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateVehicleBranchAsync(int vehicleId, int branchId)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null) return;

            vehicle.CurrentBranchId = branchId;
            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task UnassignVehicleDriverAsync(int vehicleId)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null) return;

            vehicle.CurrentDriverId = null;
            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task<List<TransferPlan>> GetPendingTransfersByBranchAsync(int branchId)
        {
            return await _context.TransferPlans
                .Include(tp => tp.FromBranch)
                .Include(tp => tp.ToBranch)
                .Include(tp => tp.Vehicle)
                    .ThenInclude(v => v!.CurrentDriver)
                .Include(tp => tp.Manager)
                .Where(tp => tp.DeletedAt == null
                    && tp.Status == "Pending"
                    && (tp.FromBranchId == branchId || tp.ToBranchId == branchId))
                .OrderByDescending(tp => tp.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int?> GetUserBranchIdAsync(int userId)
        {
            return await _context.Users
                .Where(u => u.Id == userId)
                .Select(u => u.BranchId)
                .FirstOrDefaultAsync();
        }

        public async Task<List<TransferPlan>> GetInTransitTransfersByBranchAsync(int branchId)
        {
            return await _context.TransferPlans
                .Include(tp => tp.FromBranch)
                .Include(tp => tp.ToBranch)
                .Include(tp => tp.Vehicle)
                .Include(tp => tp.Manager)
                .Include(tp => tp.TripLogs)
                    .ThenInclude(t => t.Driver)
                .Where(tp => tp.DeletedAt == null
                    && tp.Status == "InTransit"
                    && (tp.FromBranchId == branchId || tp.ToBranchId == branchId))
                .OrderByDescending(tp => tp.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task UpdateVehicleMileageAsync(int vehicleId, decimal mileage)
        {
            var vehicle = await _context.Vehicles.FirstOrDefaultAsync(v => v.Id == vehicleId);
            if (vehicle == null) return;

            vehicle.Mileage = mileage;
            vehicle.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }

        public async Task UpdateDriverBranchAsync(int driverId, int newBranchId)
        {
            var driver = await _context.Drivers.FirstOrDefaultAsync(d => d.Id == driverId);
            if (driver == null) return;

            driver.BranchId = newBranchId;
            driver.UpdatedAt = DateTime.Now;
            await _context.SaveChangesAsync();
        }
    }
}
