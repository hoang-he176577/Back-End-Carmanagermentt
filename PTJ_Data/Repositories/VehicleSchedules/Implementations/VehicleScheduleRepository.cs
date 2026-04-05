using Data.Repositories.VehicleSchedules.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.Models;

namespace Data.Repositories.VehicleSchedules.Implementations
{
    public sealed class VehicleScheduleRepository : IVehicleScheduleRepository
    {
        private readonly CarManagerContext _context;

        public VehicleScheduleRepository(CarManagerContext context)
        {
            _context = context;
        }

        public async Task<VehicleSchedule?> GetByIdAsync(int id)
        {
            return await _context.VehicleSchedules
                .Include(s => s.Vehicle).ThenInclude(v => v.Model)
                .Include(s => s.Driver)
                .Include(s => s.Branch)
                .Include(s => s.SwappedVehicle)
                .FirstOrDefaultAsync(s => s.Id == id && s.DeletedAt == null);
        }

        public async Task<VehicleSchedule> CreateAsync(VehicleSchedule schedule)
        {
            await _context.VehicleSchedules.AddAsync(schedule);
            await _context.SaveChangesAsync();
            return schedule;
        }

        public async Task UpdateAsync(VehicleSchedule schedule)
        {
            _context.VehicleSchedules.Update(schedule);
            await _context.SaveChangesAsync();
        }

        public async Task<List<VehicleSchedule>> GetSchedulesAsync(
            int branchId,
            DateTime? from,
            DateTime? to,
            int? vehicleId,
            int? driverId,
            int? modelId,
            int? seats,
            string? status)
        {
            var query = _context.VehicleSchedules
                .Include(s => s.Vehicle).ThenInclude(v => v.Model)
                .Include(s => s.Driver)
                .Include(s => s.Branch)
                .Include(s => s.SwappedVehicle)
                .Where(s => s.DeletedAt == null)
                .AsQueryable();

            if (branchId > 0)
            {
                query = query.Where(s => s.BranchId == branchId);
            }
            if (vehicleId.HasValue)
            {
                query = query.Where(s => s.VehicleId == vehicleId.Value);
            }

            if (driverId.HasValue)
            {
                query = query.Where(s => s.DriverId == driverId.Value);
            }

            if (modelId.HasValue)
            {
                query = query.Where(s => s.Vehicle.ModelId == modelId.Value);
            }

            if (seats.HasValue)
            {
                query = query.Where(s => s.Vehicle.Model != null && s.Vehicle.Model.Seats == seats.Value);
            }

            if (!string.IsNullOrWhiteSpace(status))
            {
                var normalized = status.Trim();
                query = query.Where(s => s.Status == normalized);
            }

            if (from.HasValue && to.HasValue)
            {
                query = query.Where(s => s.PlannedStartTime < to.Value && s.PlannedEndTime > from.Value);
            }
            else if (from.HasValue)
            {
                query = query.Where(s => s.PlannedEndTime >= from.Value);
            }
            else if (to.HasValue)
            {
                query = query.Where(s => s.PlannedStartTime <= to.Value);
            }

            return await query
                .OrderBy(s => s.PlannedStartTime)
                .ToListAsync();
        }

        public async Task<List<Vehicle>> GetAvailableVehiclesAsync(int branchId, int? modelId, DateTime start, DateTime end)
        {
            var readyStatuses = new[] { "assigned" };
            var vehiclesQuery = _context.Vehicles
                .Include(v => v.Model)
                .Include(v => v.CurrentBranch)
                .Where(v => v.DeletedAt == null
                    && v.CurrentBranchId == branchId
                    && v.CurrentDriverId != null
                    && v.Status != null
                    && readyStatuses.Contains(v.Status.Trim().ToLower()));

            if (modelId.HasValue)
            {
                vehiclesQuery = vehiclesQuery.Where(v => v.ModelId == modelId.Value);
            }

            var overlappingVehicleIds = await _context.VehicleSchedules
                .Where(s => s.DeletedAt == null
                    && s.PlannedStartTime < end
                    && s.PlannedEndTime > start
                    && s.Status != "Cancelled")
                .Select(s => s.VehicleId)
                .Distinct()
                .ToListAsync();

            return await vehiclesQuery
                .Where(v => !overlappingVehicleIds.Contains(v.Id))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<Driver>> GetAvailableDriversAsync(int branchId, DateTime start, DateTime end)
        {
            var driversQuery = _context.Drivers
                .Include(d => d.Branch)
                .Where(d => d.DeletedAt == null && d.BranchId == branchId);

            var overlappingDriverIds = await _context.VehicleSchedules
                .Where(s => s.DeletedAt == null
                    && s.PlannedStartTime < end
                    && s.PlannedEndTime > start
                    && s.Status != "Cancelled")
                .Select(s => s.DriverId)
                .Distinct()
                .ToListAsync();

            return await driversQuery
                .Where(d => !overlappingDriverIds.Contains(d.Id))
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Vehicle?> GetVehicleWithModelAsync(int vehicleId)
        {
            return await _context.Vehicles
                .Include(v => v.Model)
                .Include(v => v.CurrentBranch)
                .FirstOrDefaultAsync(v => v.Id == vehicleId && v.DeletedAt == null);
        }

        public async Task<Driver?> GetDriverAsync(int driverId)
        {
            return await _context.Drivers
                .Include(d => d.Branch)
                .FirstOrDefaultAsync(d => d.Id == driverId && d.DeletedAt == null);
        }

        public async Task<Branch?> GetBranchAsync(int branchId)
        {
            return await _context.Branches.FirstOrDefaultAsync(b => b.Id == branchId && b.DeletedAt == null);
        }

        public async Task AddAuditAsync(VehicleScheduleAudit audit)
        {
            await _context.VehicleScheduleAudits.AddAsync(audit);
            await _context.SaveChangesAsync();
        }

        public async Task<List<VehicleScheduleAudit>> GetAuditsByScheduleAsync(int scheduleId)
        {
            return await _context.VehicleScheduleAudits
                .Include(a => a.ActorUser)
                .Where(a => a.ScheduleId == scheduleId)
                .OrderByDescending(a => a.CreatedAt)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<List<VehicleScheduleAudit>> GetRecentAuditsAsync(int branchId, DateTime? from, DateTime? to)
        {
            var query = _context.VehicleScheduleAudits
                .Include(a => a.ActorUser)
                .Include(a => a.Schedule)
                .Where(a => branchId == 0 || a.Schedule.BranchId == branchId)
                .AsQueryable();

            if (from.HasValue)
            {
                query = query.Where(a => a.CreatedAt >= from.Value);
            }

            if (to.HasValue)
            {
                query = query.Where(a => a.CreatedAt <= to.Value);
            }

            return await query
                .OrderByDescending(a => a.CreatedAt)
                .Take(200)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<VehicleSchedule?> GetLastScheduleBeforeAsync(int vehicleId, DateTime startTime, int? excludeScheduleId)
        {
            var query = _context.VehicleSchedules
                .Where(s => s.DeletedAt == null && s.VehicleId == vehicleId && s.PlannedEndTime <= startTime);

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(s => s.Id != excludeScheduleId.Value);
            }

            return await query
                .OrderByDescending(s => s.PlannedEndTime)
                .FirstOrDefaultAsync();
        }

        public async Task<bool> HasVehicleOverlapAsync(int vehicleId, DateTime start, DateTime end, int? excludeScheduleId)
        {
            var query = _context.VehicleSchedules
                .Where(s => s.DeletedAt == null
                    && s.VehicleId == vehicleId
                    && s.PlannedStartTime < end
                    && s.PlannedEndTime > start
                    && s.Status != "Cancelled");

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(s => s.Id != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<bool> HasDriverOverlapAsync(int driverId, DateTime start, DateTime end, int? excludeScheduleId)
        {
            var query = _context.VehicleSchedules
                .Where(s => s.DeletedAt == null
                    && s.DriverId == driverId
                    && s.PlannedStartTime < end
                    && s.PlannedEndTime > start
                    && s.Status != "Cancelled");

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(s => s.Id != excludeScheduleId.Value);
            }

            return await query.AnyAsync();
        }

        public async Task<VehicleSchedule?> GetNextScheduleAsync(int vehicleId, DateTime afterTime, int? excludeScheduleId)
        {
            var query = _context.VehicleSchedules
                .Include(s => s.Vehicle)
                .Where(s => s.DeletedAt == null
                    && s.VehicleId == vehicleId
                    && s.PlannedStartTime >= afterTime
                    && s.Status != "Cancelled");

            if (excludeScheduleId.HasValue)
            {
                query = query.Where(s => s.Id != excludeScheduleId.Value);
            }

            return await query
                .OrderBy(s => s.PlannedStartTime)
                .FirstOrDefaultAsync();
        }
    }
}
