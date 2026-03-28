using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Data.Repositories.Interfaces;
using Models.DTO.VehicleDistribution;
using Models.Models;
using Service.Services.Auth.Interfaces;
using Service.Services.Common;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public sealed class VehicleDistributionService : IVehicleDistributionService
{
    private readonly IVehicleDistributionRepository _repository;
    private readonly IEmailSender _emailSender;

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Checkout", "Checkin", "Cancelled"
    };

    public VehicleDistributionService(IVehicleDistributionRepository repository, IEmailSender emailSender)
    {
        _repository = repository;
        _emailSender = emailSender;
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

        // Send email notification to operators (failures are logged but don't block response)
        try
        {
            await SendTransferNotificationEmailsAsync(dto);
        }
        catch
        {
            // Email failure must not affect the API response
        }

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

    // ───────────────── Email Notification ─────────────────

    private async Task SendTransferNotificationEmailsAsync(TransferPlanDto dto)
    {
        var branchIds = new List<int>();
        if (dto.FromBranchId.HasValue) branchIds.Add(dto.FromBranchId.Value);
        if (dto.ToBranchId.HasValue) branchIds.Add(dto.ToBranchId.Value);
        if (branchIds.Count == 0) return;

        // Check if vehicle has a driver
        var driverInfo = dto.VehicleId.HasValue
            ? await _repository.GetVehicleDriverInfoAsync(dto.VehicleId.Value)
            : (DriverId: (int?)null, DriverEmail: (string?)null, DriverName: (string?)null);
        var hasDriver = driverInfo.DriverId.HasValue && !string.IsNullOrWhiteSpace(driverInfo.DriverEmail);

        // 1. Send to operators (urgent banner if no driver)
        var operatorEmails = await _repository.GetOperatorEmailsByBranchIdsAsync(branchIds.ToArray());
        if (operatorEmails.Count > 0)
        {
            var subj = hasDriver
                ? $"[CarManagement] Yêu cầu điều chuyển xe {dto.LicensePlate ?? "N/A"}"
                : $"[CarManagement] ⚠️ KHẨN CẤP - Điều chuyển xe {dto.LicensePlate ?? "N/A"} - Cần gán tài xế!";
            var body = BuildOperatorEmailBody(dto, !hasDriver);
            foreach (var email in operatorEmails)
            {
                try { await _emailSender.SendEmailAsync(email, subj, body); } catch { }
            }
        }

        // 2. Send to driver (if assigned)
        if (hasDriver)
        {
            try
            {
                var subj = $"[CarManagement] Thông báo chuyến điều chuyển xe {dto.LicensePlate ?? "N/A"}";
                var body = BuildDriverEmailBody(dto, driverInfo.DriverName);
                await _emailSender.SendEmailAsync(driverInfo.DriverEmail!, subj, body);
            }
            catch { }
        }
    }

    /// <summary>Send email to driver when they are newly assigned to a vehicle with a pending transfer.</summary>
    public async Task SendDriverAssignedTransferEmailAsync(int vehicleId)
    {
        var plans = await _repository.GetTransferPlansAsync(null, null, "Pending");
        var plan = plans.FirstOrDefault(p => p.VehicleId == vehicleId);
        if (plan == null) return;

        var driverInfo = await _repository.GetVehicleDriverInfoAsync(vehicleId);
        if (string.IsNullOrWhiteSpace(driverInfo.DriverEmail)) return;

        try
        {
            var subj = $"[CarManagement] Thông báo chuyến điều chuyển xe {plan.LicensePlate ?? "N/A"}";
            var body = BuildDriverEmailBody(plan, driverInfo.DriverName);
            await _emailSender.SendEmailAsync(driverInfo.DriverEmail!, subj, body);
        }
        catch { }
    }

    private static string BuildOperatorEmailBody(TransferPlanDto dto, bool showDriverUrgency)
    {
        var departure = dto.PlannedDepartureDate?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
        var arrival = dto.PlannedArrivalDate?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
        var urgency = showDriverUrgency
            ? @"<div style=""background:#fff2f0;border:1px solid #ffccc7;border-radius:6px;padding:12px 16px;margin-bottom:16px;"">
        <strong style=""color:#cf1322;"">⚠️ KHẨN CẤP: Xe chưa có tài xế!</strong>
        <p style=""color:#cf1322;margin:4px 0 0;"">Vui lòng gán tài xế cho xe này ngay để đảm bảo chuyến điều chuyển đúng kế hoạch.</p>
      </div>" : "";

        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head>
<body style=""font-family:Arial,sans-serif;background:#f4f6f8;padding:20px;"">
<div style=""max-width:600px;margin:auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.1);"">
  <div style=""background:#1677ff;color:#fff;padding:20px 24px;""><h2 style=""margin:0;font-size:18px;"">📋 Yêu cầu điều chuyển xe mới</h2></div>
  <div style=""padding:24px;"">
    <p style=""color:#333;margin-top:0;"">Xin chào,</p>
    {urgency}
    <p style=""color:#333;"">Một yêu cầu điều chuyển xe mới đã được tạo bởi Ban Giám đốc:</p>
    <table style=""width:100%;border-collapse:collapse;margin:16px 0;"">
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;width:40%;"">🚗 Biển số xe</td><td style=""padding:10px 8px;font-weight:bold;color:#333;"">{dto.LicensePlate ?? "N/A"}</td></tr>
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;"">📍 Chi nhánh gốc</td><td style=""padding:10px 8px;font-weight:bold;color:#333;"">{dto.FromBranchName ?? "N/A"}</td></tr>
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;"">🏁 Chi nhánh đích</td><td style=""padding:10px 8px;font-weight:bold;color:#333;"">{dto.ToBranchName ?? "N/A"}</td></tr>
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;"">📅 Khởi hành dự kiến</td><td style=""padding:10px 8px;font-weight:bold;color:#1677ff;"">{departure}</td></tr>
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;"">📅 Đến nơi dự kiến</td><td style=""padding:10px 8px;font-weight:bold;color:#1677ff;"">{arrival}</td></tr>
      <tr><td style=""padding:10px 8px;color:#666;"">👤 Người tạo</td><td style=""padding:10px 8px;font-weight:bold;color:#333;"">{dto.ManagerName ?? "N/A"}</td></tr>
    </table>
    <p style=""color:#333;"">Vui lòng đăng nhập hệ thống để xem chi tiết.</p>
    <p style=""color:#999;font-size:12px;margin-bottom:0;"">Đây là email tự động, vui lòng không trả lời.</p>
  </div>
</div></body></html>";
    }

    private static string BuildDriverEmailBody(TransferPlanDto dto, string? driverName)
    {
        var departure = dto.PlannedDepartureDate?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";
        var arrival = dto.PlannedArrivalDate?.ToString("dd/MM/yyyy HH:mm") ?? "N/A";

        return $@"<!DOCTYPE html><html><head><meta charset=""utf-8""></head>
<body style=""font-family:Arial,sans-serif;background:#f4f6f8;padding:20px;"">
<div style=""max-width:600px;margin:auto;background:#fff;border-radius:8px;overflow:hidden;box-shadow:0 2px 8px rgba(0,0,0,.1);"">
  <div style=""background:#52c41a;color:#fff;padding:20px 24px;""><h2 style=""margin:0;font-size:18px;"">🚗 Thông báo chuyến điều chuyển xe</h2></div>
  <div style=""padding:24px;"">
    <p style=""color:#333;margin-top:0;"">Xin chào <strong>{driverName ?? "Tài xế"}</strong>,</p>
    <p style=""color:#333;"">Bạn được giao nhiệm vụ điều chuyển xe theo kế hoạch dưới đây. Vui lòng chuẩn bị sẵn sàng.</p>
    <table style=""width:100%;border-collapse:collapse;margin:16px 0;"">
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;width:40%;"">🚗 Biển số xe</td><td style=""padding:10px 8px;font-weight:bold;color:#333;"">{dto.LicensePlate ?? "N/A"}</td></tr>
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;"">📍 Xuất phát</td><td style=""padding:10px 8px;font-weight:bold;color:#333;"">{dto.FromBranchName ?? "N/A"}</td></tr>
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;"">🏁 Đến</td><td style=""padding:10px 8px;font-weight:bold;color:#333;"">{dto.ToBranchName ?? "N/A"}</td></tr>
      <tr style=""border-bottom:1px solid #eee;""><td style=""padding:10px 8px;color:#666;"">📅 Khởi hành dự kiến</td><td style=""padding:10px 8px;font-weight:bold;color:#52c41a;"">{departure}</td></tr>
      <tr><td style=""padding:10px 8px;color:#666;"">📅 Đến nơi dự kiến</td><td style=""padding:10px 8px;font-weight:bold;color:#52c41a;"">{arrival}</td></tr>
    </table>
    <p style=""color:#333;"">Nếu có thắc mắc, vui lòng liên hệ Operator chi nhánh của bạn.</p>
    <p style=""color:#999;font-size:12px;margin-bottom:0;"">Đây là email tự động, vui lòng không trả lời.</p>
  </div>
</div></body></html>";
    }
}
