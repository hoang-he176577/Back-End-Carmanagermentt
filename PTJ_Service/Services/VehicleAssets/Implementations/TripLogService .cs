using Data.Repositories.VehicleAssets.Interfaces;
using Models.DTO.PurchaseProposal;
using Models.DTO.Vehicles;
using Models.DTO.VehicleDistribution;
using Models.Models;
using Service.Exceptions;
using Service.Services.VehicleAssets.Interfaces;

namespace Service.Services.VehicleAssets.Implementations
{
    public class TripLogService : ITripLogService
    {
        private static readonly TimeSpan MaxTripDuration = TimeSpan.FromHours(24);

        private readonly ITripLogRepository _tripRepo;

        public TripLogService(ITripLogRepository tripRepo)
        {
            _tripRepo = tripRepo;
        }

        // ═══════════════ Start Trip (= Checkout) ═══════════════
        public async Task<int> StartTripAsync(StartTripRequestDto dto)
        {
            if (dto == null)
                throw BusinessErrors.BadRequest("Request body is required.");

            var now = DateTime.Now;

            // 1. Validate transfer plan exists and is Pending
            var transferPlan = await _tripRepo.GetTransferPlanWithDetailsAsync(dto.TransferPlanId);
            if (transferPlan == null)
                throw BusinessErrors.NotFound($"Transfer plan with ID {dto.TransferPlanId} not found.");

            if (!string.Equals(transferPlan.Status, "Pending", StringComparison.OrdinalIgnoreCase))
                throw BusinessErrors.BadRequest($"Transfer plan #{dto.TransferPlanId} is not in Pending status (current: {transferPlan.Status}).");

            // 2. Validate operator is at the source branch
            var operatorBranchId = await _tripRepo.GetUserBranchIdAsync(dto.OperatorId);
            if (operatorBranchId == null || operatorBranchId != transferPlan.FromBranchId)
                throw BusinessErrors.BadRequest("Only operator at the source branch can start this trip.");

            // 3. Get vehicle and driver from transfer plan
            if (!transferPlan.VehicleId.HasValue)
                throw BusinessErrors.BadRequest("Transfer plan has no vehicle assigned.");

            var vehicleId = transferPlan.VehicleId.Value;
            var vehicle = transferPlan.Vehicle;
            if (vehicle == null)
                throw BusinessErrors.NotFound($"Vehicle not found.");

            var driverId = vehicle.CurrentDriverId;
            if (!driverId.HasValue)
                throw BusinessErrors.BadRequest("Vehicle has no driver assigned. Please assign a driver first.");

            // 4. Check vehicle doesn't already have a running trip
            var runningTrip = await _tripRepo.GetRunningTripByVehicleIdAsync(vehicleId);
            if (runningTrip != null)
                throw BusinessErrors.BadRequest($"Vehicle already has an active trip (Trip #{runningTrip.Id}).");

            // 5. Block early departure — cannot start before the planned date
            if (transferPlan.PlannedDepartureDate.HasValue
                && now.Date < transferPlan.PlannedDepartureDate.Value.Date)
            {
                throw BusinessErrors.BadRequest(
                    $"Cannot start trip before the planned departure date " +
                    $"({transferPlan.PlannedDepartureDate.Value:dd/MM/yyyy}). " +
                    $"Please wait until the planned date.");
            }

            // 5a. Check late departure justification
            if (transferPlan.PlannedDepartureDate.HasValue && now > transferPlan.PlannedDepartureDate.Value)
            {
                if (string.IsNullOrWhiteSpace(dto.Note))
                    throw BusinessErrors.BadRequest("Departure is late. A justification note is required.");
            }

            // 6. Get start mileage from vehicle
            var startMileage = vehicle.Mileage ?? 0m;

            // 6. Auto-fill origin/destination from transfer plan branches
            var origin = transferPlan.FromBranch?.Name ?? "Unknown";
            var destination = transferPlan.ToBranch?.Name ?? "Unknown";

            // 7. Create trip log
            var trip = new TripLog
            {
                TransferPlanId = dto.TransferPlanId,
                VehicleId = vehicleId,
                DriverId = driverId.Value,
                StartTime = now,
                StartMileage = startMileage,
                Origin = origin,
                Destination = destination,
                StartedBy = dto.OperatorId,
                CreatedAt = now
            };

            var created = await _tripRepo.CreateAsync(trip);

            // 8. Update transfer plan → InTransit (= Checkout)
            transferPlan.Status = "InTransit";
            transferPlan.CheckoutDate = now;
            transferPlan.CheckoutByUserId = dto.OperatorId;
            transferPlan.CheckoutNote = dto.Note;
            transferPlan.UpdatedAt = now;
            await _tripRepo.UpdateTransferPlanAsync(transferPlan);

            // 9. Update vehicle status → Moving
            await _tripRepo.UpdateVehicleStatusAsync(vehicleId, "Moving");

            // 10. Unassign driver (driver stays at source branch)
            await _tripRepo.UnassignVehicleDriverAsync(vehicleId);

            return created.Id;
        }

        // ═══════════════ End Trip (= Checkin) ═══════════════
        public async Task EndTripAsync(int tripId, EndTripRequestDto dto)
        {
            var trip = await _tripRepo.GetByIdAsync(tripId);
            if (trip == null)
                throw BusinessErrors.NotFound("Trip not found.");

            if (trip.EndTime != null)
                throw BusinessErrors.BadRequest("Trip already ended.");

            if (dto.EndMileage < trip.StartMileage)
                throw BusinessErrors.BadRequest("End mileage must be >= start mileage.");

            var endTime = DateTime.Now;
            if (endTime - trip.StartTime > MaxTripDuration)
                throw BusinessErrors.BadRequest("Trip duration exceeded the allowed limit.");

            // 1. Validate operator is at the destination branch
            var transferPlan = trip.TransferPlan;
            if (transferPlan != null && dto.EndedBy > 0)
            {
                var operatorBranchId = await _tripRepo.GetUserBranchIdAsync(dto.EndedBy);
                if (operatorBranchId == null || operatorBranchId != transferPlan.ToBranchId)
                    throw BusinessErrors.BadRequest("Only operator at the destination branch can end this trip.");

                // Check late arrival justification
                if (transferPlan.PlannedArrivalDate.HasValue && endTime > transferPlan.PlannedArrivalDate.Value)
                {
                    if (string.IsNullOrWhiteSpace(dto.Note))
                        throw BusinessErrors.BadRequest("Arrival is late. A justification note is required.");
                }
            }

            // 2. Update trip log
            trip.EndTime = endTime;
            trip.EndMileage = dto.EndMileage;
            trip.EndedBy = dto.EndedBy;
            await _tripRepo.UpdateAsync(trip);

            // 3. Update transfer plan → Completed (= Checkin)
            if (transferPlan != null)
            {
                transferPlan.Status = "Completed";
                transferPlan.CheckinDate = endTime;
                transferPlan.CheckinByUserId = dto.EndedBy;
                transferPlan.CheckinNote = dto.Note;
                transferPlan.ExecutedDate = DateOnly.FromDateTime(endTime);
                transferPlan.UpdatedAt = endTime;
                await _tripRepo.UpdateTransferPlanAsync(transferPlan);

                // 4. Move vehicle to destination branch
                if (transferPlan.VehicleId.HasValue && transferPlan.ToBranchId.HasValue)
                {
                    await _tripRepo.UpdateVehicleBranchAsync(
                        transferPlan.VehicleId.Value, transferPlan.ToBranchId.Value);
                }

                // 5. Move driver to destination branch along with vehicle
                if (transferPlan.ToBranchId.HasValue)
                {
                    await _tripRepo.UpdateDriverBranchAsync(
                        trip.DriverId, transferPlan.ToBranchId.Value);
                }
            }

            // 5. Update vehicle status → Active and mileage
            await _tripRepo.UpdateVehicleStatusAsync(trip.VehicleId, "Active");
            await _tripRepo.UpdateVehicleMileageAsync(trip.VehicleId, dto.EndMileage);
        }

        // ═══════════════ Pending Transfers ═══════════════
        public async Task<List<PendingTransferDto>> GetPendingTransfersAsync(int branchId)
        {
            var plans = await _tripRepo.GetPendingTransfersByBranchAsync(branchId);
            return plans.Select(tp => new PendingTransferDto
            {
                TransferPlanId = tp.Id,
                VehicleId = tp.VehicleId,
                LicensePlate = tp.Vehicle?.LicensePlate,
                FromBranchId = tp.FromBranchId,
                FromBranchName = tp.FromBranch?.Name,
                ToBranchId = tp.ToBranchId,
                ToBranchName = tp.ToBranch?.Name,
                DriverId = tp.Vehicle?.CurrentDriverId,
                DriverName = tp.Vehicle?.CurrentDriver?.Name,
                CurrentMileage = tp.Vehicle?.Mileage,
                PlannedDepartureDate = tp.PlannedDepartureDate,
                PlannedArrivalDate = tp.PlannedArrivalDate,
                IsSourceBranch = tp.FromBranchId == branchId,
            }).ToList();
        }

        // ═══════════════ InTransit Transfers ═══════════════
        public async Task<List<PendingTransferDto>> GetInTransitTransfersAsync(int branchId)
        {
            var plans = await _tripRepo.GetInTransitTransfersByBranchAsync(branchId);
            return plans.Select(tp =>
            {
                var tripLog = tp.TripLogs?.OrderByDescending(t => t.StartTime).FirstOrDefault();
                return new PendingTransferDto
                {
                    TransferPlanId = tp.Id,
                    VehicleId = tp.VehicleId,
                    LicensePlate = tp.Vehicle?.LicensePlate,
                    FromBranchId = tp.FromBranchId,
                    FromBranchName = tp.FromBranch?.Name,
                    ToBranchId = tp.ToBranchId,
                    ToBranchName = tp.ToBranch?.Name,
                    DriverId = tripLog?.DriverId,
                    DriverName = tripLog?.Driver?.Name,
                    CurrentMileage = tripLog?.StartMileage,
                    PlannedDepartureDate = tp.PlannedDepartureDate,
                    PlannedArrivalDate = tp.PlannedArrivalDate,
                    IsSourceBranch = tp.FromBranchId == branchId,
                    TripId = tripLog?.Id,
                    StartTime = tripLog?.StartTime,
                };
            }).ToList();
        }

        // ═══════════════ Queries (unchanged) ═══════════════
        public async Task<List<TripHistoryResponseDto>> GetAllTripHistoryAsync()
        {
            var trips = await _tripRepo.GetTripHistoryByVehicleAsync(null);
            return trips.Select(MapTripHistory).ToList();
        }

        public async Task<TripHistoryByVehicleResponseDto> GetVehicleTripHistoryAsync(int vehicleId)
        {
            var vehicle = await _tripRepo.GetVehicleByIdAsync(vehicleId);
            if (vehicle == null)
                throw BusinessErrors.NotFound($"Vehicle with ID {vehicleId} not found.");

            var trips = await _tripRepo.GetTripHistoryByVehicleAsync(vehicleId);

            return new TripHistoryByVehicleResponseDto
            {
                VehicleId = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Status = vehicle.Status,
                CurrentBranchId = vehicle.CurrentBranchId,
                CurrentBranchName = vehicle.CurrentBranch?.Name,
                CurrentDriverId = vehicle.CurrentDriverId,
                CurrentDriverName = vehicle.CurrentDriver?.Name,
                Trips = trips.Select(MapTripHistory).ToList()
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

        // ═══════════════ Helpers ═══════════════
        private static TripHistoryResponseDto MapTripHistory(TripLog trip)
        {
            return new TripHistoryResponseDto
            {
                TripId = trip.Id,
                TransferPlanId = trip.TransferPlanId,
                FromBranchName = trip.TransferPlan?.FromBranch?.Name,
                ToBranchName = trip.TransferPlan?.ToBranch?.Name,
                VehicleId = trip.VehicleId,
                VehicleLicensePlate = trip.Vehicle?.LicensePlate,
                DriverId = trip.DriverId,
                DriverName = trip.Driver?.Name,
                StartTime = trip.StartTime,
                EndTime = trip.EndTime,
                StartMileage = trip.StartMileage,
                EndMileage = trip.EndMileage,
                Origin = trip.Origin,
                Destination = trip.Destination
            };
        }

        private static List<ManageVehicleTripDto> FilterManageVehicles(List<ManageVehicleTripDto> items, string? tab)
        {
            if (string.IsNullOrWhiteSpace(tab)) return items;

            return tab.Trim().ToLowerInvariant() switch
            {
                "moving" => items.Where(x => x.IsMoving).ToList(),
                _ => items,
            };
        }
    }
}
