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
        "Approved", "Rejected", "Executed", "Cancelled"
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

        // Validate existence
        if (!await _repository.VehicleExistsAsync(request.VehicleId.Value))
            return ServiceResult<TransferPlanDto>.Fail(400, "Vehicle not found.");

        if (!await _repository.BranchExistsAsync(request.FromBranchId.Value))
            return ServiceResult<TransferPlanDto>.Fail(400, "FromBranch not found.");

        if (!await _repository.BranchExistsAsync(request.ToBranchId.Value))
            return ServiceResult<TransferPlanDto>.Fail(400, "ToBranch not found.");

        // Validate that the creator belongs to the fromBranch
        var userBranchId = await _repository.GetUserBranchIdAsync(managerId);
        if (userBranchId == null || userBranchId != request.FromBranchId)
            return ServiceResult<TransferPlanDto>.Fail(403, "You can only create transfer plans from your own branch.");

        // Check no active transfer for this vehicle
        if (await _repository.HasActiveTransferAsync(request.VehicleId.Value))
            return ServiceResult<TransferPlanDto>.Fail(409, "Vehicle already has a pending or approved transfer.");

        var plan = new TransferPlan
        {
            VehicleId = request.VehicleId,
            FromBranchId = request.FromBranchId,
            ToBranchId = request.ToBranchId,
            ManagerId = managerId,
            PlanDate = request.PlanDate,
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
                "Invalid status. Allowed: Approved, Rejected, Executed, Cancelled.");

        var plan = await _repository.GetTransferPlanEntityAsync(id);
        if (plan == null)
            return ServiceResult<TransferPlanDto>.Fail(404, "Transfer plan not found.");

        // ── State-machine validation ──
        var current = plan.Status ?? string.Empty;
        var error = ValidateTransition(current, newStatus, userRole, userId, plan.ManagerId);
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
            if (string.Equals(newStatus, "Approved", StringComparison.OrdinalIgnoreCase))
            {
                // Xe bắt đầu trong trạng thái điều chuyển
                await _repository.UpdateVehicleStatusAsync(plan.VehicleId.Value, "InTransfer");
            }
        }
        else if (string.Equals(newStatus, "Checkin", StringComparison.OrdinalIgnoreCase))
        {
            // Operator at destination branch confirms vehicle arrival
            plan.Status = "Completed";
            plan.CheckinDate = DateTime.Now;
            plan.CheckinByUserId = userId;
            plan.ExecutedDate = DateOnly.FromDateTime(DateTime.Now);

                // Chuyển xe sang chi nhánh mới và đặt lại Active
                if (plan.ToBranchId.HasValue)
                {
                    await _repository.UpdateVehicleBranchAsync(
                        plan.VehicleId.Value, plan.ToBranchId.Value);
                }
                await _repository.UpdateVehicleStatusAsync(plan.VehicleId.Value, "Active");
            }
            else if (string.Equals(newStatus, "Rejected", StringComparison.OrdinalIgnoreCase)
                  || string.Equals(newStatus, "Cancelled", StringComparison.OrdinalIgnoreCase))
            {
                // Nếu xe đang ở trạng thái InTransfer thì reset lại Active
                if (string.Equals(current, "Approved", StringComparison.OrdinalIgnoreCase))
                {
                    await _repository.UpdateVehicleStatusAsync(plan.VehicleId.Value, "Active");
                }
            }
        }

        await _repository.UpdateTransferPlanAsync(plan);

        var dto = await _repository.GetTransferPlanByIdAsync(id);
        return ServiceResult<TransferPlanDto>.SuccessResult(dto!);
    }

    // ───────────────── Helpers ─────────────────

    private static string? ValidateTransition(
        string currentStatus, string newStatus, string userRole, int userId, int? planManagerId)
    {
        var isExec = string.Equals(userRole, "Executive Management", StringComparison.OrdinalIgnoreCase);
        var isAccountant = string.Equals(userRole, "Branch Asset Accountant", StringComparison.OrdinalIgnoreCase);
        var isOperator = string.Equals(userRole, "Operator", StringComparison.OrdinalIgnoreCase);

        switch (currentStatus)
        {
            case "Pending":
                if (newStatus is "Approved" or "Rejected")
                {
                    if (!isExec)
                        return "Only Executive Management can approve or reject a transfer.";
                    return null;
                }
                if (newStatus == "Cancelled")
                {
                    if (isExec || (isAccountant && planManagerId == userId))
                        return null;
                    return "You do not have permission to cancel this transfer.";
                }
                return $"Cannot transition from Pending to {newStatus}.";

            case "Approved":
                if (newStatus == "Executed")
                {
                    if (!isOperator)
                        return "Only Operator can execute a transfer.";
                    return null;
                }
                if (newStatus == "Cancelled")
                {
                    if (isExec)
                        return null;
                    return "Only Executive Management can cancel an approved transfer.";
                }
                return $"Cannot transition from Approved to {newStatus}.";

            default:
                return $"Transfer plan is already '{currentStatus}' and cannot be updated.";
        }
    }
}
