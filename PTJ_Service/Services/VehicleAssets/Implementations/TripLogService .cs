using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Services.VehicleAssets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace Service.Services.VehicleAssets.Implementations
{
    public class TripLogService : ITripLogsService
    {
        private readonly CarManagerContext _context;
        private readonly ITripLogRepository _repo;

        private static readonly TimeSpan MaxTripDuration = TimeSpan.FromHours(24);
        private const int RemainingNearThresholdMinutes = 30;

        private readonly ITripLogRepository _tripRepo;

        public TripLogService(ITripLogRepository tripRepo)
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

            var lastDriverTrip = await _tripRepo.GetLastCompletedTripByDriverIdAsync(dto.DriverId);
            if (lastDriverTrip?.EndMileage != null && dto.StartMileage < lastDriverTrip.EndMileage.Value)
            {
                throw new Exception("Start mileage cannot be less than current mileage");
            }

            var plannedMinutes = NormalizeDurationMinutes(dto.PlannedDurationDays, dto.PlannedDurationHours, dto.PlannedDurationMinutes);
            if (plannedMinutes == null || plannedMinutes <= 0)
            {
                throw BusinessErrors.BadRequest("Planned duration is required.");
            }

            var meta = new TripLogMeta
            {
                PurposeText = dto.Purpose,
                PlannedDurationMinutes = plannedMinutes
            };

            var trip = new TripLog
            {
                VehicleId = dto.VehicleId,
                DriverId = dto.DriverId,
                StartTime = startTime,
                StartMileage = dto.StartMileage,
                Origin = dto.Origin,
                Destination = dto.Destination,
                Purpose = dto.Purpose,
                StartedBy = dto.OperatorId,
                CreatedAt = now,
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

            var endTime = DateTime.Now;
            if (endTime - trip.StartTime > MaxTripDuration)
            {
                throw BusinessErrors.BadRequest("Trip duration exceeded the allowed limit.");
            }

            trip.EndTime = endTime;
            trip.EndMileage = dto.EndMileage;
            trip.EndedBy = dto.EndedBy;

            if (vehicle != null)
            {
                vehicle.Mileage = request.EndMileage;
            }

        public async Task<TripHistoryByVehicleResponseDto> GetVehicleTripHistoryAsync(int vehicleId)
        {
            var vehicle = await _tripRepo.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw BusinessErrors.NotFound($"Vehicle with ID {vehicleId} not found.");
            }

            var trips = await _tripRepo.GetTripHistoryByVehicleAsync(vehicleId);
            var now = DateTime.Now;

            var tripDtos = trips.Select(t => new TripHistoryResponseDto
            {
                TripId = t.Id,
                VehicleId = t.VehicleId,
                DriverId = t.DriverId,
                DriverName = t.Driver.Name!,
                StartTime = t.StartTime,
                EndTime = t.EndTime,
                StartMileage = t.StartMileage,
                EndMileage = t.EndMileage,
                Origin = t.Origin,
                Destination = t.Destination,
                Purpose = t.Purpose
            }).ToList();

            return new TripHistoryByVehicleResponseDto
            {
                VehicleId = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Status = vehicle.Status,
                CurrentBranchId = vehicle.CurrentBranchId,
                CurrentBranchName = vehicle.CurrentBranch?.Name,
                CurrentDriverId = vehicle.CurrentDriverId,
                CurrentDriverName = vehicle.CurrentDriver?.Name,
                Trips = tripDtos
            };
        }

        public async Task<List<ListVehicleDrop>> GetVehicleDropAsync()
        {
            var list = await _tripRepo.GetVehiclesDropAsync();
            return list.Select(v => new ListVehicleDrop
            {
                Id = v.Id,
                Name = v.LicensePlate,
            }).ToList();
        }

        public async Task<UserBasicDto> GetDriverByVehicleIdAsync(int vehicleId)
            => await _tripRepo.GetDriverByVehicleIdAsync(vehicleId) ??
            throw BusinessErrors.NotFound($"No driver found for vehicle with ID {vehicleId}.");

        public async Task<List<ManageVehicleTripDto>> GetManageVehiclesAsync(int branchId, string? tab)
        {
            var vehicles = await _tripRepo.GetVehiclesByBranchAsync(branchId);
            var runningTrips = await _tripRepo.GetRunningTripsByBranchAsync(branchId);

            var runningByVehicleId = runningTrips
                .GroupBy(t => t.VehicleId)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.StartTime).First());

            var now = DateTime.Now;
            var items = vehicles.Select(v =>
            {
                runningByVehicleId.TryGetValue(v.Id, out var runningTrip);
                var isMoving = runningTrip != null;
                var overDuration = isMoving && now - runningTrip!.StartTime > MaxTripDuration;

                return new ManageVehicleTripDto
                {
                    VehicleId = v.Id,
                    LicensePlate = v.LicensePlate,
                    Status = v.Status,
                    CurrentBranchId = v.CurrentBranchId,
                    CurrentBranchName = v.CurrentBranch?.Name,
                    CurrentDriverId = v.CurrentDriverId,
                    CurrentDriverName = v.CurrentDriver?.Name,
                    IsMoving = isMoving,
                    CurrentTripId = runningTrip?.Id,
                    CurrentTripStartTime = runningTrip?.StartTime,
                    CurrentTripStartMileage = runningTrip?.StartMileage,
                    CurrentTripOrigin = runningTrip?.Origin,
                    CurrentTripDestination = runningTrip?.Destination,
                    IsOverDuration = overDuration
                };
            }).ToList();

            return FilterManageVehicles(items, tab);
        }

        private static bool IsReadyStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            return ReadyStatuses.Contains(status.Trim());
        }

        private static List<ManageVehicleTripDto> FilterManageVehicles(
            List<ManageVehicleTripDto> items,
            string? tab)
        {
            if (string.IsNullOrWhiteSpace(tab))
            {
                return items;
            }

            switch (tab.Trim().ToLowerInvariant())
            {
                case "ready":
                    return items.Where(x => !x.IsMoving && IsReadyStatus(x.Status)).ToList();
                case "moving":
                    return items.Where(x => x.IsMoving).ToList();
                case "all":
                default:
                    return items;
            }
        }
    }
}
