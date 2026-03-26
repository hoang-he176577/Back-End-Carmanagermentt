using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Repositories.Interfaces;
using Models.DTO.VehicleDistribution;
using Models.Models;
using Service.Services.Common;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public sealed class VehicleDistributionService : IVehicleDistributionService
{
    private readonly IVehicleDistributionRepository _repository;

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Checkout", "Checkin", "Cancelled"
    };

    public VehicleDistributionService(IVehicleDistributionRepository repository)
    {
        _repository = repository;
    }

    // ───────────────── Queries ─────────────────

    public async Task<List<TransferPlanDto>> GetTransferPlansAsync(
        int? fromBranchId, int? toBranchId, string? status, int userId, string userRole)
    {
        // Executive Management sees all; others see only their branch
        int? userBranchId = null;
        var isExec = string.Equals(userRole, "Executive Management", StringComparison.OrdinalIgnoreCase);
        if (!isExec && userId > 0)
        {
            userBranchId = await _repository.GetUserBranchIdAsync(userId);
        }

        return await _repository.GetTransferPlansAsync(fromBranchId, toBranchId, status, userBranchId);
    }

    public Task<TransferPlanDto?> GetTransferPlanByIdAsync(int id)
    {
        return _repository.GetTransferPlanByIdAsync(id);
    }

    public Task<List<BranchStockSummaryDto>> GetBranchStockAsync()
    {
        return _repository.GetBranchStockSummariesAsync();
    }

    // ───────────────── Create ─────────────────

    public async Task<ServiceResult<TransferPlanDto>> CreateTransferPlanAsync(
        TransferPlanCreateRequestDto request, int managerId)
    {
        if (request == null)
            return ServiceResult<TransferPlanDto>.Fail(400, "Request body is required.");

        if (request.VehicleId is null or <= 0)
            return ServiceResult<TransferPlanDto>.Fail(400, "VehicleId is required.");

        if (request.FromBranchId is null or <= 0)
            return ServiceResult<TransferPlanDto>.Fail(400, "FromBranchId is required.");

        if (request.ToBranchId is null or <= 0)
            return ServiceResult<TransferPlanDto>.Fail(400, "ToBranchId is required.");

        if (request.FromBranchId == request.ToBranchId)
            return ServiceResult<TransferPlanDto>.Fail(400, "FromBranchId and ToBranchId must be different.");

        if (request.PlannedDepartureDate is null)
            return ServiceResult<TransferPlanDto>.Fail(400, "PlannedDepartureDate is required.");

        if (request.PlannedArrivalDate is null)
            return ServiceResult<TransferPlanDto>.Fail(400, "PlannedArrivalDate is required.");

        if (request.PlannedDepartureDate < DateTime.Now)
            return ServiceResult<TransferPlanDto>.Fail(400, "PlannedDepartureDate cannot be in the past.");

        if (request.PlannedArrivalDate <= request.PlannedDepartureDate)
            return ServiceResult<TransferPlanDto>.Fail(400, "PlannedArrivalDate must be after PlannedDepartureDate.");

        // Validate existence
        if (!await _repository.VehicleExistsAsync(request.VehicleId.Value))
            return ServiceResult<TransferPlanDto>.Fail(400, "Vehicle not found.");

        // Validate vehicle status — block Disposed, Moving, InTransfer
        var vehicleStatus = await _repository.GetVehicleStatusAsync(request.VehicleId.Value);
        if (!string.IsNullOrEmpty(vehicleStatus))
        {
            var blocked = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
            {
                "Disposed", "Moving", "InTransfer"
            };
            if (blocked.Contains(vehicleStatus))
                return ServiceResult<TransferPlanDto>.Fail(400,
                    $"Cannot create transfer for vehicle with status '{vehicleStatus}'.");
        }

        if (!await _repository.BranchExistsAsync(request.FromBranchId.Value))
            return ServiceResult<TransferPlanDto>.Fail(400, "FromBranch not found.");

        if (!await _repository.BranchExistsAsync(request.ToBranchId.Value))
            return ServiceResult<TransferPlanDto>.Fail(400, "ToBranch not found.");

        // Check no active transfer for this vehicle
        if (await _repository.HasActiveTransferAsync(request.VehicleId.Value))
            return ServiceResult<TransferPlanDto>.Fail(409, "Vehicle already has a pending or in-transit transfer.");

        var plan = new TransferPlan
        {
            VehicleId = request.VehicleId,
            FromBranchId = request.FromBranchId,
            ToBranchId = request.ToBranchId,
            ManagerId = managerId,
            PlannedDepartureDate = request.PlannedDepartureDate,
            PlannedArrivalDate = request.PlannedArrivalDate,
            Status = "Pending"
        };

        var created = await _repository.AddTransferPlanAsync(plan);
        var dto = await _repository.GetTransferPlanByIdAsync(created.Id);

        if (dto == null)
            return ServiceResult<TransferPlanDto>.Fail(500, "Failed to load created transfer plan.");

        return ServiceResult<TransferPlanDto>.SuccessResult(dto, 201);
    }

    // ───────────────── Update Status ─────────────────

    public async Task<ServiceResult<TransferPlanDto>> UpdateTransferPlanStatusAsync(
        int id, TransferPlanUpdateStatusDto request, int userId, string userRole)
    {
        if (request == null || string.IsNullOrWhiteSpace(request.Status))
            return ServiceResult<TransferPlanDto>.Fail(400, "Status is required.");

        var newStatus = request.Status.Trim();

        if (!AllowedStatuses.Contains(newStatus))
            return ServiceResult<TransferPlanDto>.Fail(400,
                "Invalid status. Allowed: Checkout, Checkin, Cancelled.");

        var plan = await _repository.GetTransferPlanEntityAsync(id);
        if (plan == null)
            return ServiceResult<TransferPlanDto>.Fail(404, "Transfer plan not found.");

        // ── State-machine validation ──
        var current = plan.Status ?? string.Empty;
        var userBranchId = await _repository.GetUserBranchIdAsync(userId);
        var error = ValidateTransition(current, newStatus, userRole, userBranchId, plan.FromBranchId, plan.ToBranchId);
        if (error != null)
            return ServiceResult<TransferPlanDto>.Fail(403, error);

        // ── Apply state change ──
        if (string.Equals(newStatus, "Checkout", StringComparison.OrdinalIgnoreCase))
        {
            // Operator at source branch confirms vehicle departure
            plan.Status = "InTransit";
            plan.CheckoutDate = DateTime.Now;
            plan.CheckoutByUserId = userId;

            if (plan.VehicleId.HasValue)
            {
                await _repository.UpdateVehicleStatusAsync(plan.VehicleId.Value, "InTransfer");
                // Gỡ tài xế — tài xế ở lại chi nhánh cũ, xe đi không có tài xế
                await _repository.UnassignVehicleDriverAsync(plan.VehicleId.Value);
            }
        }
        else if (string.Equals(newStatus, "Checkin", StringComparison.OrdinalIgnoreCase))
        {
            // Operator at destination branch confirms vehicle arrival
            plan.Status = "Completed";
            plan.CheckinDate = DateTime.Now;
            plan.CheckinByUserId = userId;
            plan.ExecutedDate = DateOnly.FromDateTime(DateTime.Now);

            if (plan.VehicleId.HasValue)
            {
                // Move vehicle to new branch and set Active
                if (plan.ToBranchId.HasValue)
                {
                    await _repository.UpdateVehicleBranchAsync(
                        plan.VehicleId.Value, plan.ToBranchId.Value);

                    // Move driver to destination branch along with vehicle
                    var driverId = await _repository.GetVehicleTransferDriverIdAsync(plan.VehicleId.Value);
                    if (driverId.HasValue)
                    {
                        await _repository.UpdateDriverBranchAsync(driverId.Value, plan.ToBranchId.Value);
                    }
                }
                await _repository.UpdateVehicleStatusAsync(plan.VehicleId.Value, "Active");
            }
        }
        else if (string.Equals(newStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
        {
            plan.Status = "Cancelled";

            // If vehicle was already InTransfer (from checkout), reset to Active
            if (plan.VehicleId.HasValue
                && string.Equals(current, "InTransit", StringComparison.OrdinalIgnoreCase))
            {
                await _repository.UpdateVehicleStatusAsync(plan.VehicleId.Value, "Active");
            }
        }

        await _repository.UpdateTransferPlanAsync(plan);

        var dto = await _repository.GetTransferPlanByIdAsync(id);
        return ServiceResult<TransferPlanDto>.SuccessResult(dto!);
    }

    // ───────────────── Helpers ─────────────────

    private static string? ValidateTransition(
        string currentStatus, string newStatus, string userRole,
        int? userBranchId, int? fromBranchId, int? toBranchId)
    {
        var isExec = string.Equals(userRole, "Executive Management", StringComparison.OrdinalIgnoreCase);
        var isOperator = string.Equals(userRole, "Operator", StringComparison.OrdinalIgnoreCase);

        switch (currentStatus)
        {
            case "Pending":
                if (string.Equals(newStatus, "Checkout", StringComparison.OrdinalIgnoreCase))
                {
                    if (!isOperator)
                        return "Only Operator can perform checkout.";
                    if (userBranchId == null || userBranchId != fromBranchId)
                        return "Only Operator at the source branch can perform checkout.";
                    return null;
                }
                if (string.Equals(newStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    if (!isExec)
                        return "Only Executive Management can cancel a transfer.";
                    return null;
                }
                return $"Cannot transition from Pending to {newStatus}.";

            case "InTransit":
                if (string.Equals(newStatus, "Checkin", StringComparison.OrdinalIgnoreCase))
                {
                    if (!isOperator)
                        return "Only Operator can perform checkin.";
                    if (userBranchId == null || userBranchId != toBranchId)
                        return "Only Operator at the destination branch can perform checkin.";
                    return null;
                }
                if (string.Equals(newStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
                {
                    if (!isExec)
                        return "Only Executive Management can cancel a transfer.";
                    return null;
                }
                return $"Cannot transition from InTransit to {newStatus}.";

            default:
                return $"Transfer plan is already '{currentStatus}' and cannot be updated.";
        }
    }
}
