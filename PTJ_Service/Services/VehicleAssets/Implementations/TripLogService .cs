using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.PurchaseProposal;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Exceptions;
using Service.Services.VehicleAssets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Service.Services.VehicleAssets.Implementations
{
    public class TripLogService : ITripLogService
    {
        private static readonly HashSet<string> ReadyStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Available",
            "Assigned",
            "Active"
        };

        private static readonly TimeSpan MaxTripDuration = TimeSpan.FromHours(24);

        private readonly ITripLogRepository _tripRepo;

        public TripLogService(ITripLogRepository tripRepo)
        {
            _tripRepo = tripRepo;
        }

        public async Task<int> StartTripAsync(StartTripRequestDto dto)
        {
            if (dto == null)
            {
                throw BusinessErrors.BadRequest("Request body is required.");
            }

            var now = DateTime.Now;

            var vehicle = await _tripRepo.GetVehicleByIdAsync(dto.VehicleId);
            if (vehicle == null)
            {
                throw BusinessErrors.NotFound($"Vehicle with ID {dto.VehicleId} not found.");
            }

            if (!IsReadyStatus(vehicle.Status))
            {
                throw BusinessErrors.BadRequest($"Vehicle with ID {dto.VehicleId} is not ready for trip.");
            }

            var runningTrip = await _tripRepo.GetRunningTripByVehicleIdAsync(dto.VehicleId);
            if (runningTrip != null) { 
                throw BusinessErrors.BadRequest($"Vehicle with ID {dto.VehicleId} already has an active trip.");
            }

            var startTime = now;

            var lastDriverTrip = await _tripRepo.GetLastCompletedTripByDriverIdAsync(dto.DriverId);
            if (lastDriverTrip?.EndMileage != null && dto.StartMileage < lastDriverTrip.EndMileage.Value)
            {
                throw BusinessErrors.BadRequest("Start mileage must be greater than or equal to the previous trip's end mileage for this driver.");
            }

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
            trip.StartedBy = dto.OperatorId;

            var created = await _tripRepo.CreateAsync(trip);
            await _tripRepo.UpdateVehicleStatusAsync(dto.VehicleId, "Moving");

            return created.Id;
        }

        public async Task EndTripAsync(int tripId, EndTripRequestDto dto)
        {
            var trip = await _tripRepo.GetByIdAsync(tripId);

            if (trip == null)
                throw BusinessErrors.NotFound("Trip not found");

            if (trip.EndTime != null)
                throw BusinessErrors.BadRequest("Trip already ended");

            if (dto.EndMileage < trip.StartMileage)
            {
                throw BusinessErrors.BadRequest("End mileage must be greater than or equal to start mileage.");
            }

            var endTime = DateTime.Now;
            if (endTime - trip.StartTime > MaxTripDuration)
            {
                throw BusinessErrors.BadRequest("Trip duration exceeded the allowed limit.");
            }

            trip.EndTime = endTime;
            trip.EndMileage = dto.EndMileage;
            trip.EndedBy = dto.EndedBy;

            await _tripRepo.UpdateAsync(trip);

            var vehicle = await _tripRepo.GetVehicleByIdAsync(trip.VehicleId);
            if (vehicle != null)
            {
                var nextStatus = vehicle.CurrentDriverId.HasValue ? "Assigned" : "Available";
                await _tripRepo.UpdateVehicleStatusAsync(trip.VehicleId, nextStatus);
            }
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
