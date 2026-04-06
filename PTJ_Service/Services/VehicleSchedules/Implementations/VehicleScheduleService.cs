using Data.Repositories.VehicleSchedules.Interfaces;
using Models.DTO.Schedules;
using Models.Models;
using Service.Exceptions;
using Service.Services.VehicleSchedules.Interfaces;

namespace Service.Services.VehicleSchedules.Implementations
{
    public sealed class VehicleScheduleService : IVehicleScheduleService
    {
        private readonly IVehicleScheduleRepository _repository;
        private static readonly string[] ReadyVehicleStatuses = { "Assigned" };

        public VehicleScheduleService(IVehicleScheduleRepository repository)
        {
            _repository = repository;
        }

        public async Task<List<VehicleScheduleDto>> GetSchedulesAsync(
            int branchId,
            DateTime? from,
            DateTime? to,
            int? vehicleId,
            int? driverId,
            int? modelId,
            int? seats,
            string? status)
        {
            var schedules = await _repository.GetSchedulesAsync(branchId, from, to, vehicleId, driverId, modelId, seats, status);
            return schedules.Select(MapToDto).ToList();
        }

        public async Task<VehicleScheduleDto> CreateScheduleAsync(int branchId, int actorUserId, VehicleScheduleCreateRequestDto request)
        {
            if (request == null)
            {
                throw BusinessErrors.BadRequest("Request body is required.");
            }

            if (request.VehicleId <= 0 || request.DriverId <= 0)
            {
                throw BusinessErrors.BadRequest("VehicleId and DriverId are required.");
            }

            if (request.PlannedEndTime <= request.PlannedStartTime)
            {
                throw BusinessErrors.BadRequest("PlannedEndTime must be after PlannedStartTime.");
            }
            if (request.PlannedStartTime <= DateTime.Now)
            {
                throw BusinessErrors.BadRequest("Thời gian bắt đầu phải lớn hơn thời gian hiện tại.");
            }

            if (branchId != 0 && request.BranchId.HasValue && request.BranchId.Value != branchId)
            {
                throw BusinessErrors.BadRequest("BranchId does not match your branch.");
            }

            var vehicle = await _repository.GetVehicleWithModelAsync(request.VehicleId);
            if (vehicle == null)
            {
                throw BusinessErrors.NotFound("Vehicle not found.");
            }

            if (branchId != 0 && (!vehicle.CurrentBranchId.HasValue || vehicle.CurrentBranchId.Value != branchId))
            {
                throw BusinessErrors.BadRequest("Vehicle does not belong to your branch.");
            }
            if (!IsVehicleReadyForSchedule(vehicle))
            {
                throw BusinessErrors.BadRequest("Xe chưa sẵn sàng hoặc chưa được phân công tài xế.");
            }

            var driver = await _repository.GetDriverAsync(request.DriverId);
            if (driver == null)
            {
                throw BusinessErrors.NotFound("Driver not found.");
            }

            if (branchId != 0 && (!driver.BranchId.HasValue || driver.BranchId.Value != branchId))
            {
                throw BusinessErrors.BadRequest("Driver does not belong to your branch.");
            }

            var hasVehicleOverlap = await _repository.HasVehicleOverlapAsync(
                request.VehicleId,
                request.PlannedStartTime,
                request.PlannedEndTime,
                excludeScheduleId: null);
            if (hasVehicleOverlap)
            {
                throw BusinessErrors.Conflict("Vehicle already has a schedule overlapping this time range.");
            }

            var hasDriverOverlap = await _repository.HasDriverOverlapAsync(
                request.DriverId,
                request.PlannedStartTime,
                request.PlannedEndTime,
                excludeScheduleId: null);
            if (hasDriverOverlap)
            {
                throw BusinessErrors.Conflict("Driver already has a schedule overlapping this time range.");
            }

            var effectiveBranchId = branchId != 0 ? branchId : (request.BranchId ?? vehicle.CurrentBranchId ?? 0);
            if (effectiveBranchId == 0)
            {
                throw BusinessErrors.BadRequest("BranchId is required.");
            }
            var branch = await _repository.GetBranchAsync(effectiveBranchId);
            var branchName = branch?.Name?.Trim();
            if (string.IsNullOrWhiteSpace(branchName))
            {
                throw BusinessErrors.BadRequest("Không xác định được chi nhánh của xe.");
            }
            var origin = request.Origin?.Trim();
            var destination = request.Destination?.Trim();
            if (string.IsNullOrWhiteSpace(origin) || string.IsNullOrWhiteSpace(destination))
            {
                throw BusinessErrors.BadRequest("Điểm đón khách và điểm trả khách là bắt buộc.");
            }
            if (string.Equals(origin, branchName, StringComparison.OrdinalIgnoreCase))
            {
                throw BusinessErrors.BadRequest("Điểm đón khách trùng tên chi nhánh. Vui lòng ghi cụ thể.");
            }
            if (string.Equals(destination, branchName, StringComparison.OrdinalIgnoreCase))
            {
                throw BusinessErrors.BadRequest("Điểm trả khách trùng tên chi nhánh. Vui lòng ghi cụ thể.");
            }

            var schedule = new VehicleSchedule
            {
                VehicleId = request.VehicleId,
                DriverId = request.DriverId,
                BranchId = effectiveBranchId,
                PlannedStartTime = request.PlannedStartTime,
                PlannedEndTime = request.PlannedEndTime,
                Origin = origin,
                Destination = destination,
                Status = "Planned",
                CreatedAt = DateTime.Now
            };

            var created = await _repository.CreateAsync(schedule);
            var loaded = await _repository.GetByIdAsync(created.Id) ?? created;
            await _repository.AddAuditAsync(new VehicleScheduleAudit
            {
                ScheduleId = created.Id,
                ActorUserId = actorUserId,
                Action = "Create",
                Note = "Created schedule",
                DataJson = $"{{\"vehicleId\":{created.VehicleId},\"driverId\":{created.DriverId}}}",
                CreatedAt = DateTime.Now
            });
            return MapToDto(loaded);
        }

        public async Task<VehicleScheduleDto> UpdateScheduleAsync(int scheduleId, int branchId, int actorUserId, VehicleScheduleUpdateRequestDto request)
        {
            if (request == null)
            {
                throw BusinessErrors.BadRequest("Request body is required.");
            }

            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                throw BusinessErrors.NotFound("Schedule not found.");
            }

            if (branchId != 0 && schedule.BranchId != branchId)
            {
                throw BusinessErrors.Unauthorized("You can only manage schedules in your branch.");
            }
            var effectiveBranchId = branchId != 0 ? branchId : schedule.BranchId;


            var plannedStart = request.PlannedStartTime ?? schedule.PlannedStartTime;
            var plannedEnd = request.PlannedEndTime ?? schedule.PlannedEndTime;
            if (plannedEnd <= plannedStart)
            {
                throw BusinessErrors.BadRequest("PlannedEndTime must be after PlannedStartTime.");
            }
            if (plannedStart <= DateTime.Now)
            {
                throw BusinessErrors.BadRequest("Thời gian bắt đầu phải lớn hơn thời gian hiện tại.");
            }

            var vehicleId = request.VehicleId ?? schedule.VehicleId;
            var driverId = request.DriverId ?? schedule.DriverId;

            var vehicle = await _repository.GetVehicleWithModelAsync(vehicleId);
            if (vehicle == null)
            {
                throw BusinessErrors.NotFound("Vehicle not found.");
            }
            if (branchId != 0 && (!vehicle.CurrentBranchId.HasValue || vehicle.CurrentBranchId.Value != branchId))
            {
                throw BusinessErrors.BadRequest("Vehicle does not belong to your branch.");
            }
            if (!IsVehicleReadyForSchedule(vehicle))
            {
                throw BusinessErrors.BadRequest("Xe chưa sẵn sàng hoặc chưa được phân công tài xế.");
            }

            if (request.DriverId.HasValue)
            {
                var driver = await _repository.GetDriverAsync(driverId);
                if (driver == null)
                {
                    throw BusinessErrors.NotFound("Driver not found.");
                }
                if (branchId != 0 && (!driver.BranchId.HasValue || driver.BranchId.Value != branchId))
                {
                    throw BusinessErrors.BadRequest("Driver does not belong to your branch.");
                }
            }

            if (await _repository.HasVehicleOverlapAsync(vehicleId, plannedStart, plannedEnd, schedule.Id))
            {
                throw BusinessErrors.Conflict("Vehicle already has a schedule overlapping this time range.");
            }

            if (await _repository.HasDriverOverlapAsync(driverId, plannedStart, plannedEnd, schedule.Id))
            {
                throw BusinessErrors.Conflict("Driver already has a schedule overlapping this time range.");
            }

            var branch = await _repository.GetBranchAsync(schedule.BranchId);
            var branchName = branch?.Name?.Trim();
            if (string.IsNullOrWhiteSpace(branchName))
            {
                throw BusinessErrors.BadRequest("Không xác định được chi nhánh của xe.");
            }

            schedule.VehicleId = vehicleId;
            schedule.DriverId = driverId;
            schedule.PlannedStartTime = plannedStart;
            schedule.PlannedEndTime = plannedEnd;
            if (request.Origin != null)
            {
                var origin = request.Origin.Trim();
                if (string.Equals(origin, branchName, StringComparison.OrdinalIgnoreCase))
                {
                    throw BusinessErrors.BadRequest("Điểm đón khách trùng tên chi nhánh. Vui lòng ghi cụ thể.");
                }
                schedule.Origin = origin;
            }
            if (request.Destination != null)
            {
                var destination = request.Destination.Trim();
                if (string.Equals(destination, branchName, StringComparison.OrdinalIgnoreCase))
                {
                    throw BusinessErrors.BadRequest("Điểm trả khách trùng tên chi nhánh. Vui lòng ghi cụ thể.");
                }
                schedule.Destination = destination;
            }
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                schedule.Status = request.Status.Trim();
            }
            schedule.UpdatedAt = DateTime.Now;

            await _repository.UpdateAsync(schedule);
            var loaded = await _repository.GetByIdAsync(schedule.Id) ?? schedule;
            await _repository.AddAuditAsync(new VehicleScheduleAudit
            {
                ScheduleId = schedule.Id,
                ActorUserId = actorUserId,
                Action = "Update",
                Note = "Updated schedule",
                DataJson = $"{{\"vehicleId\":{schedule.VehicleId},\"driverId\":{schedule.DriverId}}}",
                CreatedAt = DateTime.Now
            });
            return MapToDto(loaded);
        }

        public async Task<VehicleScheduleExtendResultDto> ExtendScheduleAsync(
            int scheduleId,
            int branchId,
            int actorUserId,
            VehicleScheduleExtendRequestDto request)
        {
            if (request == null)
            {
                throw BusinessErrors.BadRequest("Request body is required.");
            }

            if (request.ExtensionMinutes <= 0)
            {
                throw BusinessErrors.BadRequest("ExtensionMinutes must be greater than 0.");
            }

            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                throw BusinessErrors.NotFound("Schedule not found.");
            }

            if (branchId != 0 && schedule.BranchId != branchId)
            {
                throw BusinessErrors.Unauthorized("You can only manage schedules in your branch.");
            }
            var effectiveBranchId = branchId != 0 ? branchId : schedule.BranchId;

            var newEnd = schedule.PlannedEndTime.AddMinutes(request.ExtensionMinutes);

            var conflict = await _repository.GetNextScheduleAsync(schedule.VehicleId, schedule.PlannedEndTime, schedule.Id);
            var hasDriverOverlap = await _repository.HasDriverOverlapAsync(schedule.DriverId, schedule.PlannedStartTime, newEnd, schedule.Id);
            if (conflict != null && conflict.PlannedStartTime < newEnd)
            {
                var available = await _repository.GetAvailableVehiclesAsync(
                    effectiveBranchId,
                    null,
                    conflict.PlannedStartTime,
                    conflict.PlannedEndTime);

                var availabilityDtos = available.Select(MapAvailability).ToList();
                var driverAvailability = await _repository.GetAvailableDriversAsync(effectiveBranchId, conflict.PlannedStartTime, conflict.PlannedEndTime);
                var driverDtos = driverAvailability.Select(MapDriverAvailability).ToList();

                if (!request.AllowSwap || !request.AllowDriverSwap)
                {
                    return new VehicleScheduleExtendResultDto
                    {
                        Extended = false,
                        Conflict = MapConflict(conflict),
                        AvailableVehicles = availabilityDtos,
                        AvailableDrivers = driverDtos
                    };
                }

                if (!request.SwapVehicleId.HasValue || !request.SwapDriverId.HasValue)
                {
                    return new VehicleScheduleExtendResultDto
                    {
                        Extended = false,
                        Conflict = MapConflict(conflict),
                        AvailableVehicles = availabilityDtos,
                        AvailableDrivers = driverDtos
                    };
                }

                var swapVehicleId = request.SwapVehicleId.Value;
                var swapDriverId = request.SwapDriverId.Value;
                if (!available.Any(v => v.Id == swapVehicleId))
                {
                    throw BusinessErrors.BadRequest("Selected swap vehicle is not available.");
                }
                if (!driverAvailability.Any(d => d.Id == swapDriverId))
                {
                    throw BusinessErrors.BadRequest("Selected swap driver is not available.");
                }

                var oldVehicleId = conflict.VehicleId;
                var oldDriverId = conflict.DriverId;
                conflict.VehicleId = swapVehicleId;
                conflict.DriverId = swapDriverId;
                conflict.SwappedVehicleId = oldVehicleId;
                conflict.SwapFromScheduleId = schedule.Id;
                conflict.UpdatedAt = DateTime.Now;
                await _repository.UpdateAsync(conflict);
                await _repository.AddAuditAsync(new VehicleScheduleAudit
                {
                    ScheduleId = conflict.Id,
                    ActorUserId = actorUserId,
                    Action = "SwapVehicleDriver",
                    Note = "Swapped vehicle and driver due to extension conflict",
                    DataJson = $"{{\"oldVehicleId\":{oldVehicleId},\"newVehicleId\":{swapVehicleId},\"oldDriverId\":{oldDriverId},\"newDriverId\":{swapDriverId}}}",
                    CreatedAt = DateTime.Now
                });
                hasDriverOverlap = false;
            }

            if (hasDriverOverlap)
            {
                var availableDrivers = await _repository.GetAvailableDriversAsync(effectiveBranchId, schedule.PlannedStartTime, newEnd);
                var driverDtos = availableDrivers.Select(MapDriverAvailability).ToList();

                return new VehicleScheduleExtendResultDto
                {
                    Extended = false,
                    AvailableDrivers = driverDtos
                };
            }

            schedule.PlannedEndTime = newEnd;
            schedule.ExtensionMinutes = (schedule.ExtensionMinutes ?? 0) + request.ExtensionMinutes;
            if (!string.IsNullOrWhiteSpace(request.ExtensionReason))
            {
                schedule.ExtensionReason = request.ExtensionReason.Trim();
            }
            schedule.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(schedule);
            await _repository.AddAuditAsync(new VehicleScheduleAudit
            {
                ScheduleId = schedule.Id,
                ActorUserId = actorUserId,
                Action = "Extend",
                Note = request.ExtensionReason,
                DataJson = $"{{\"extensionMinutes\":{request.ExtensionMinutes}}}",
                CreatedAt = DateTime.Now
            });

            var loaded = await _repository.GetByIdAsync(schedule.Id) ?? schedule;
            return new VehicleScheduleExtendResultDto
            {
                Extended = true,
                Schedule = MapToDto(loaded)
            };
        }

        public async Task<List<VehicleScheduleAvailabilityDto>> GetAvailableVehiclesAsync(
            int branchId,
            int? modelId,
            DateTime start,
            DateTime end)
        {
            if (end <= start)
            {
                throw BusinessErrors.BadRequest("End time must be after start time.");
            }

            var vehicles = await _repository.GetAvailableVehiclesAsync(branchId, modelId, start, end);
            return vehicles.Select(MapAvailability).ToList();
        }

        public async Task<List<DriverAvailabilityDto>> GetAvailableDriversAsync(int branchId, DateTime start, DateTime end)
        {
            if (end <= start)
            {
                throw BusinessErrors.BadRequest("End time must be after start time.");
            }

            var drivers = await _repository.GetAvailableDriversAsync(branchId, start, end);
            return drivers.Select(MapDriverAvailability).ToList();
        }

        public async Task<VehicleScheduleDto> StartScheduleAsync(int scheduleId, int branchId, int actorUserId, VehicleScheduleActionRequestDto request)
        {
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                throw BusinessErrors.NotFound("Schedule not found.");
            }

            if (branchId != 0 && schedule.BranchId != branchId)
            {
                throw BusinessErrors.Unauthorized("You can only manage schedules in your branch.");
            }

            if (!string.Equals(schedule.Status, "Planned", StringComparison.OrdinalIgnoreCase))
            {
                throw BusinessErrors.BadRequest("Schedule is not in Planned status.");
            }

            var actual = request.ActualTime ?? DateTime.Now;
            schedule.ActualStartTime = actual;
            schedule.Status = "InProgress";
            schedule.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(schedule);
            var loaded = await _repository.GetByIdAsync(schedule.Id) ?? schedule;
            await _repository.AddAuditAsync(new VehicleScheduleAudit
            {
                ScheduleId = schedule.Id,
                ActorUserId = actorUserId,
                Action = "Start",
                Note = request.Note,
                DataJson = $"{{\"actualStart\":\"{actual:O}\"}}",
                CreatedAt = DateTime.Now
            });
            return MapToDto(loaded);
        }

        public async Task<VehicleScheduleDto> EndScheduleAsync(int scheduleId, int branchId, int actorUserId, VehicleScheduleActionRequestDto request)
        {
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                throw BusinessErrors.NotFound("Schedule not found.");
            }

            if (branchId != 0 && schedule.BranchId != branchId)
            {
                throw BusinessErrors.Unauthorized("You can only manage schedules in your branch.");
            }

            if (!string.Equals(schedule.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
            {
                throw BusinessErrors.BadRequest("Schedule is not in InProgress status.");
            }

            var actual = request.ActualTime ?? DateTime.Now;
            schedule.ActualEndTime = actual;
            schedule.Status = "Completed";
            schedule.UpdatedAt = DateTime.Now;
            await _repository.UpdateAsync(schedule);
            var loaded = await _repository.GetByIdAsync(schedule.Id) ?? schedule;
            await _repository.AddAuditAsync(new VehicleScheduleAudit
            {
                ScheduleId = schedule.Id,
                ActorUserId = actorUserId,
                Action = "End",
                Note = request.Note,
                DataJson = $"{{\"actualEnd\":\"{actual:O}\"}}",
                CreatedAt = DateTime.Now
            });
            return MapToDto(loaded);
        }

        public async Task<List<VehicleScheduleAuditDto>> GetAuditsByScheduleAsync(int scheduleId, int branchId)
        {
            var schedule = await _repository.GetByIdAsync(scheduleId);
            if (schedule == null)
            {
                throw BusinessErrors.NotFound("Schedule not found.");
            }

            if (branchId != 0 && schedule.BranchId != branchId)
            {
                throw BusinessErrors.Unauthorized("You can only access schedules in your branch.");
            }

            var audits = await _repository.GetAuditsByScheduleAsync(scheduleId);
            return audits.Select(MapAudit).ToList();
        }

        public async Task<List<VehicleScheduleAuditDto>> GetRecentAuditsAsync(int branchId, DateTime? from, DateTime? to)
        {
            var audits = await _repository.GetRecentAuditsAsync(branchId, from, to);
            return audits.Select(MapAudit).ToList();
        }

        private static VehicleScheduleDto MapToDto(VehicleSchedule schedule)
        {
            return new VehicleScheduleDto
            {
                Id = schedule.Id,
                VehicleId = schedule.VehicleId,
                VehicleLicensePlate = schedule.Vehicle?.LicensePlate,
                VehicleModelId = schedule.Vehicle?.ModelId,
                VehicleModelName = schedule.Vehicle?.Model?.ModelName,
                VehicleManufacturer = schedule.Vehicle?.Model?.Manufacturer,
                DriverId = schedule.DriverId,
                DriverName = schedule.Driver?.Name,
                BranchId = schedule.BranchId,
                BranchName = schedule.Branch?.Name,
                PlannedStartTime = schedule.PlannedStartTime,
                PlannedEndTime = schedule.PlannedEndTime,
                ActualStartTime = schedule.ActualStartTime,
                ActualEndTime = schedule.ActualEndTime,
                Origin = schedule.Origin,
                Destination = schedule.Destination,
                Status = schedule.Status,
                ExtensionMinutes = schedule.ExtensionMinutes,
                ExtensionReason = schedule.ExtensionReason,
                SwapFromScheduleId = schedule.SwapFromScheduleId,
                SwappedVehicleId = schedule.SwappedVehicleId,
                SwappedVehicleLicensePlate = schedule.SwappedVehicle?.LicensePlate,
                CreatedAt = schedule.CreatedAt,
                UpdatedAt = schedule.UpdatedAt
            };
        }

        private static VehicleScheduleAvailabilityDto MapAvailability(Vehicle vehicle)
        {
            return new VehicleScheduleAvailabilityDto
            {
                VehicleId = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                ModelId = vehicle.ModelId,
                ModelName = vehicle.Model?.ModelName,
                Manufacturer = vehicle.Model?.Manufacturer,
                BranchName = vehicle.CurrentBranch?.Name,
                Status = vehicle.Status,
                CurrentDriverId = vehicle.CurrentDriverId
            };
        }

        private static DriverAvailabilityDto MapDriverAvailability(Driver driver)
        {
            return new DriverAvailabilityDto
            {
                DriverId = driver.Id,
                DriverName = driver.Name,
                Phone = driver.Phone,
                Email = driver.Email,
                BranchName = driver.Branch?.Name,
                Status = driver.Status
            };
        }

        private static VehicleScheduleAuditDto MapAudit(VehicleScheduleAudit audit)
        {
            return new VehicleScheduleAuditDto
            {
                Id = audit.Id,
                ScheduleId = audit.ScheduleId,
                ActorUserId = audit.ActorUserId,
                ActorName = audit.ActorUser?.Name ?? audit.ActorUser?.Email,
                Action = audit.Action,
                Note = audit.Note,
                DataJson = audit.DataJson,
                CreatedAt = audit.CreatedAt
            };
        }

        private static VehicleScheduleConflictDto MapConflict(VehicleSchedule schedule)
        {
            return new VehicleScheduleConflictDto
            {
                ScheduleId = schedule.Id,
                PlannedStartTime = schedule.PlannedStartTime,
                PlannedEndTime = schedule.PlannedEndTime,
                VehicleId = schedule.VehicleId,
                VehicleLicensePlate = schedule.Vehicle?.LicensePlate
            };
        }

        private static bool IsVehicleReadyForSchedule(Vehicle vehicle)
        {
            if (!vehicle.CurrentDriverId.HasValue)
            {
                return false;
            }
            var status = vehicle.Status?.Trim();
            if (string.IsNullOrWhiteSpace(status))
            {
                return false;
            }
            return ReadyVehicleStatuses.Any(s => status.Equals(s, StringComparison.OrdinalIgnoreCase));
        }
    }
}

