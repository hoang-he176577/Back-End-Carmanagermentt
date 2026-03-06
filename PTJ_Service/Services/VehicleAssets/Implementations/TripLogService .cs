using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Services.VehicleAssets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.VehicleAssets.Implementations
{
    public class TripLogService : ITripLogsService
    {
        private readonly CarManagerContext _context;
        private readonly ITripLogRepository _repo;

        public TripLogService(
            CarManagerContext context,
            ITripLogRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        public async Task<List<TripLog>> GetAllAsync()
        {
            return await _repo.GetAllAsync();
        }

        public async Task<TripLog?> GetByIdAsync(int id)
        {
            return await _repo.GetByIdAsync(id);
        }

        public async Task<bool> StartTripAsync(StartTripRequestDto request)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == request.VehicleId);

            if (vehicle == null)
                throw new Exception("Vehicle not found");

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == request.DriverId);

            if (driver == null)
                throw new Exception("Driver not found");

            var hasActiveTrip = await _repo.HasActiveTripAsync(request.VehicleId);

            if (hasActiveTrip)
                throw new Exception("Vehicle already has active trip");

            if (vehicle.Mileage.HasValue &&
                request.StartMileage < vehicle.Mileage.Value)
            {
                throw new Exception("Start mileage cannot be less than current mileage");
            }

            var trip = new TripLog
            {
                VehicleId = request.VehicleId,
                DriverId = request.DriverId,
                StartTime = DateTime.UtcNow,
                StartMileage = request.StartMileage,
                Origin = request.Origin,
                Destination = request.Destination,
                Purpose = request.Purpose,
                CreatedAt = DateTime.UtcNow
            };

            await _repo.AddAsync(trip);
            await _repo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EndTripAsync(int tripId, EndTripRequestDto request)
        {
            var trip = await _repo.GetByIdAsync(tripId);

            if (trip == null)
                throw new Exception("Trip not found");

            if (trip.EndTime != null)
                throw new Exception("Trip already ended");

            if (request.EndMileage < trip.StartMileage)
                throw new Exception("End mileage must be greater than start mileage");

            trip.EndMileage = request.EndMileage;
            trip.EndTime = DateTime.UtcNow;

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == trip.VehicleId);

            if (vehicle != null)
            {
                vehicle.Mileage = request.EndMileage;
            }

            await _repo.SaveChangesAsync();

            return true;
        }
    }
}
