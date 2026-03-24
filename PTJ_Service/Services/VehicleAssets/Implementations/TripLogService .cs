using Data.Repositories.VehicleAssets.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Exceptions;
using Service.Services.VehicleAssets.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
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

        private static readonly HashSet<string> ReadyStatuses = new(StringComparer.OrdinalIgnoreCase)
        {
            "Active",
            "Available",
            "Ready",
            "Idle",
            "Assigned"
        };

        public TripLogService(CarManagerContext context, ITripLogRepository repo)
        {
            _context = context;
            _repo = repo;
        }

        public Task<List<TripLog>> GetAllAsync()
        {
            return _repo.GetAllAsync();
        }

        public Task<TripLog?> GetByIdAsync(int id)
        {
            return _repo.GetByIdAsync(id);
        }

        public async Task<bool> StartTripAsync(StartTripRequestDto request)
        {
            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == request.VehicleId);

            if (vehicle == null)
            {
                throw BusinessErrors.NotFound("Vehicle not found");
            }

            var driver = await _context.Drivers
                .FirstOrDefaultAsync(d => d.Id == request.DriverId);

            if (driver == null)
            {
                throw BusinessErrors.NotFound("Driver not found");
            }

            if (await _repo.HasActiveTripAsync(request.VehicleId))
            {
                throw BusinessErrors.Conflict("Vehicle already has an active trip.");
            }

            var lastDriverTrip = await _repo.GetLastCompletedTripByDriverIdAsync(request.DriverId);
            if (lastDriverTrip?.EndMileage != null && request.StartMileage < lastDriverTrip.EndMileage.Value)
            {
                throw BusinessErrors.BadRequest("Start mileage cannot be less than last end mileage.");
            }

            if (vehicle.Mileage != null && request.StartMileage < vehicle.Mileage.Value)
            {
                throw BusinessErrors.BadRequest("Start mileage cannot be less than current vehicle mileage.");
            }

            var plannedMinutes = NormalizeDurationMinutes(request.PlannedDurationDays, request.PlannedDurationHours, request.PlannedDurationMinutes);
            if (plannedMinutes == null || plannedMinutes <= 0)
            {
                throw BusinessErrors.BadRequest("Planned duration is required.");
            }

            if (request.OperatorId <= 0)
            {
                throw BusinessErrors.BadRequest("Operator is required.");
            }

            var meta = new TripLogMeta
            {
                PurposeText = request.Purpose,
                PlannedDurationMinutes = plannedMinutes
            };

            var now = DateTime.Now;
            var trip = new TripLog
            {
                VehicleId = request.VehicleId,
                DriverId = request.DriverId,
                StartTime = now,
                StartMileage = request.StartMileage,
                Origin = request.Origin,
                Destination = request.Destination,
                Purpose = SerializeMeta(meta),
                StartedBy = request.OperatorId,
                CreatedAt = now
            };

            await _repo.AddAsync(trip);
            await _repo.SaveChangesAsync();

            return true;
        }

        public async Task<bool> EndTripAsync(int tripId, EndTripRequestDto request)
        {
            var trip = await _repo.GetByIdAsync(tripId);

            if (trip == null)
            {
                throw BusinessErrors.NotFound("Trip not found");
            }

            if (trip.EndTime != null)
            {
                throw BusinessErrors.BadRequest("Trip already ended");
            }

            if (trip.StartMileage != null && request.EndMileage < trip.StartMileage.Value)
            {
                throw BusinessErrors.BadRequest("End mileage must be greater than start mileage");
            }

            var endTime = DateTime.Now;
            if (trip.StartTime != null && endTime - trip.StartTime > MaxTripDuration)
            {
                throw BusinessErrors.BadRequest("Trip duration exceeded the allowed limit.");
            }

            var meta = ParseMeta(trip.Purpose);
            meta.IsStopDifferent = request.IsStopDifferent;
            meta.ActualStop = request.ActualStop;
            meta.StopDeviationReason = request.StopDeviationReason;
            meta.OvertimeReason = request.OvertimeReason;
            meta.ExtensionMinutes = NormalizeDurationMinutes(request.ExtensionDays, request.ExtensionHours, request.ExtensionMinutes);

            trip.EndTime = endTime;
            trip.EndMileage = request.EndMileage;
            trip.Purpose = SerializeMeta(meta);

            var vehicle = await _context.Vehicles
                .FirstOrDefaultAsync(v => v.Id == trip.VehicleId);

            if (vehicle != null)
            {
                vehicle.Mileage = request.EndMileage;
            }

            await _repo.UpdateAsync(trip);
            await _repo.SaveChangesAsync();

            return true;
        }

        public async Task<List<ManageVehicleTripDto>> GetManageVehiclesAsync(int branchId, string? tab)
        {
            var vehicles = await _repo.GetVehiclesByBranchAsync(branchId);
            var runningTrips = await _repo.GetRunningTripsByBranchAsync(branchId);

            var runningByVehicleId = runningTrips
                .Where(t => t.VehicleId.HasValue)
                .GroupBy(t => t.VehicleId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.StartTime).First());

            var vehicleIds = vehicles.Select(v => v.Id).ToList();
            var lastTrips = await _repo.GetLastCompletedTripsByVehicleIdsAsync(vehicleIds);
            var lastTripByVehicleId = lastTrips
                .Where(t => t.VehicleId.HasValue)
                .GroupBy(t => t.VehicleId!.Value)
                .ToDictionary(g => g.Key, g => g.OrderByDescending(x => x.EndTime).First());

            var now = DateTime.Now;

            var items = vehicles.Select(v =>
            {
                runningByVehicleId.TryGetValue(v.Id, out var runningTrip);
                var isMoving = runningTrip != null;
                var overDuration = isMoving && runningTrip!.StartTime != null && now - runningTrip.StartTime > MaxTripDuration;

                TripLogMeta? runningMeta = null;
                if (isMoving)
                {
                    runningMeta = ParseMeta(runningTrip!.Purpose);
                }

                int? plannedDuration = runningMeta?.PlannedDurationMinutes;
                int? remainingMinutes = null;
                string? remainingStatus = null;

                if (isMoving && plannedDuration != null && runningTrip!.StartTime != null)
                {
                    var elapsedMinutes = (int)Math.Floor((now - runningTrip.StartTime.Value).TotalMinutes);
                    remainingMinutes = plannedDuration.Value - elapsedMinutes;

                    if (remainingMinutes < 0)
                    {
                        remainingStatus = "Overtime";
                    }
                    else if (remainingMinutes <= RemainingNearThresholdMinutes)
                    {
                        remainingStatus = "Near";
                    }
                    else
                    {
                        remainingStatus = "OnTime";
                    }
                }

                lastTripByVehicleId.TryGetValue(v.Id, out var lastTrip);

                return new ManageVehicleTripDto
                {
                    VehicleId = v.Id,
                    LicensePlate = v.LicensePlate,
                    Status = v.Status,
                    CurrentBranchId = v.CurrentBranchId,
                    CurrentBranchName = v.CurrentBranch?.Name,
                    CurrentDriverId = v.CurrentDriverId,
                    CurrentDriverName = v.CurrentDriver?.Name,
                    CurrentMileage = v.Mileage,
                    IsMoving = isMoving,
                    CurrentTripId = runningTrip?.Id,
                    CurrentTripStartTime = runningTrip?.StartTime,
                    CurrentTripStartMileage = runningTrip?.StartMileage,
                    CurrentTripOrigin = runningTrip?.Origin,
                    CurrentTripDestination = runningTrip?.Destination,
                    IsOverDuration = overDuration,
                    LastTripEndTime = lastTrip?.EndTime,
                    PlannedDurationMinutes = plannedDuration,
                    RemainingMinutes = remainingMinutes,
                    RemainingStatus = remainingStatus
                };
            }).ToList();

            return FilterManageVehicles(items, tab);
        }

        public async Task<TripHistoryByVehicleResponseDto> GetVehicleTripHistoryAsync(int vehicleId)
        {
            var vehicle = await _repo.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
            {
                throw BusinessErrors.NotFound($"Vehicle with ID {vehicleId} not found.");
            }

            var trips = await _repo.GetTripHistoryByVehicleAsync(vehicleId);

            var tripDtos = trips.Select(t =>
            {
                var meta = ParseMeta(t.Purpose);
                return new TripHistoryResponseDto
                {
                    TripId = t.Id,
                    VehicleId = t.VehicleId,
                    DriverId = t.DriverId,
                    DriverName = t.Driver?.Name,
                    StartTime = t.StartTime,
                    EndTime = t.EndTime,
                    StartMileage = t.StartMileage,
                    EndMileage = t.EndMileage,
                    Origin = t.Origin,
                    Destination = t.Destination,
                    Purpose = meta.PurposeText ?? t.Purpose,
                    PlannedDurationMinutes = meta.PlannedDurationMinutes,
                    IsStopDifferent = meta.IsStopDifferent,
                    ActualStop = meta.ActualStop,
                    StopDeviationReason = meta.StopDeviationReason,
                    OvertimeReason = meta.OvertimeReason,
                    ExtensionMinutes = meta.ExtensionMinutes
                };
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

        private static bool IsReadyStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }

            return ReadyStatuses.Contains(status.Trim());
        }

        private static int? NormalizeDurationMinutes(int? days, int? hours, int? minutes)
        {
            if (days == null && hours == null && minutes == null)
            {
                return null;
            }

            var total = (days ?? 0) * 24 * 60 + (hours ?? 0) * 60 + (minutes ?? 0);
            return total;
        }

        private static TripLogMeta ParseMeta(string? value)
        {
            if (string.IsNullOrWhiteSpace(value))
            {
                return new TripLogMeta();
            }

            try
            {
                var meta = JsonSerializer.Deserialize<TripLogMeta>(value);
                if (meta != null)
                {
                    return meta;
                }
            }
            catch
            {
            }

            return new TripLogMeta
            {
                PurposeText = value
            };
        }

        private static string? SerializeMeta(TripLogMeta meta)
        {
            if (meta == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(meta);
        }

        private sealed class TripLogMeta
        {
            public string? PurposeText { get; set; }
            public int? PlannedDurationMinutes { get; set; }
            public bool? IsStopDifferent { get; set; }
            public string? ActualStop { get; set; }
            public string? StopDeviationReason { get; set; }
            public string? OvertimeReason { get; set; }
            public int? ExtensionMinutes { get; set; }
        }
    }
}
