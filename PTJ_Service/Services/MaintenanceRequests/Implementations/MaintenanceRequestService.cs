using Data.Repositories.MaintenanceRequests.Interfaces;
using Models.DTO.Maintenance;
using Models.Models;
using Service.Services.Common;
using Service.Services.MaintenanceRequests.Interfaces;

namespace Service.Services.MaintenanceRequests.Implementations;

public sealed class MaintenanceRequestService : IMaintenanceRequestService
{
    private static readonly HashSet<string> AllowedTypes = new(StringComparer.OrdinalIgnoreCase)
    {
        "Periodic",
        "Breakdown"
    };

    private static readonly HashSet<string> AllowedStatuses = new(StringComparer.OrdinalIgnoreCase)
    {
        "Pending",
        "Approved",
        "Rejected",
        "InProgress",
        "Completed"
    };

    private readonly IMaintenanceRequestRepository _repository;

    public MaintenanceRequestService(IMaintenanceRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<ServiceResult<List<MaintenanceRequestDto>>> GetListAsync(string? status, string? maintenanceType, bool includeDeleted, int userId, string userRole)
    {
        if (!string.IsNullOrWhiteSpace(status) && !AllowedStatuses.Contains(status.Trim()))
        {
            return ServiceResult<List<MaintenanceRequestDto>>.Fail(400, "Invalid status.");
        }

        if (!string.IsNullOrWhiteSpace(maintenanceType) && !AllowedTypes.Contains(maintenanceType.Trim()))
        {
            return ServiceResult<List<MaintenanceRequestDto>>.Fail(400, "Invalid maintenanceType.");
        }

        // Executive Management can see all branches; others see only their branch
        int? branchId = null;
        var isExec = string.Equals(userRole, "Executive Management", StringComparison.OrdinalIgnoreCase);
        if (!isExec && userId > 0)
        {
            branchId = await _repository.GetUserBranchIdAsync(userId);
        }

        var items = await _repository.GetListAsync(status, maintenanceType, includeDeleted, branchId);
        return ServiceResult<List<MaintenanceRequestDto>>.SuccessResult(items);
    }

    public async Task<ServiceResult<MaintenanceRequestDto>> GetByIdAsync(int id, bool includeDeleted)
    {
        var item = await _repository.GetByIdAsync(id, includeDeleted);
        return item == null
            ? ServiceResult<MaintenanceRequestDto>.Fail(404, "Maintenance request not found.")
            : ServiceResult<MaintenanceRequestDto>.SuccessResult(item);
    }

    public async Task<ServiceResult<MaintenanceRequestDto>> CreateAsync(int actorUserId, MaintenanceCreateRequestDto request)
    {
        if (request == null)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "Request body is required.");
        }

        if (!request.VehicleId.HasValue || request.VehicleId.Value <= 0)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "VehicleId is required.");
        }

        if (!await _repository.VehicleExistsAsync(request.VehicleId.Value))
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "Vehicle not found.");
        }

        var vehicleStatus = await _repository.GetVehicleStatusAsync(request.VehicleId.Value);
        if (string.Equals(vehicleStatus, "Disposed", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "Xe đã được thanh lý, không thể tạo yêu cầu bảo trì.");
        }

        var type = request.MaintenanceType?.Trim();
        if (string.IsNullOrWhiteSpace(type) || !AllowedTypes.Contains(type))
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "MaintenanceType must be 'Periodic' or 'Breakdown'.");
        }

        if (request.EstimatedCost.HasValue && request.EstimatedCost.Value < 0)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "EstimatedCost must be >= 0.");
        }

        var now = DateTime.UtcNow;
        var entity = new MaintenanceRequest
        {
            VehicleId = request.VehicleId.Value,
            OperatorId = actorUserId,
            RequestDate = request.RequestDate ?? DateOnly.FromDateTime(now),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            EstimatedCost = request.EstimatedCost,
            MaintenanceType = NormalizeType(type),
            Status = "Pending",
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _repository.AddAsync(entity);
        var dto = await _repository.GetByIdAsync(created.Id, includeDeleted: true);
        if (dto == null)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(500, "Failed to load created maintenance request.");
        }

        return ServiceResult<MaintenanceRequestDto>.SuccessResult(dto, 201);
    }

    public async Task<ServiceResult<MaintenanceRequestDto>> UpdateAsync(int id, MaintenanceUpdateRequestDto request)
    {
        if (request == null)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "Request body is required.");
        }

        var entity = await _repository.GetEntityByIdAsync(id);
        if (entity == null)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(404, "Maintenance request not found.");
        }

        if (request.VehicleId.HasValue)
        {
            if (request.VehicleId.Value <= 0 || !await _repository.VehicleExistsAsync(request.VehicleId.Value))
            {
                return ServiceResult<MaintenanceRequestDto>.Fail(400, "Vehicle not found.");
            }

            var vehicleStatus = await _repository.GetVehicleStatusAsync(request.VehicleId.Value);
            if (string.Equals(vehicleStatus, "Disposed", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<MaintenanceRequestDto>.Fail(400, "Xe đã được thanh lý, không thể tạo yêu cầu bảo trì.");
            }

            entity.VehicleId = request.VehicleId.Value;
        }

        if (request.RequestDate.HasValue)
        {
            entity.RequestDate = request.RequestDate.Value;
        }

        if (request.Description != null)
        {
            entity.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        }

        if (request.EstimatedCost.HasValue)
        {
            if (request.EstimatedCost.Value < 0)
            {
                return ServiceResult<MaintenanceRequestDto>.Fail(400, "EstimatedCost must be >= 0.");
            }

            entity.EstimatedCost = request.EstimatedCost.Value;
        }

        if (!string.IsNullOrWhiteSpace(request.MaintenanceType))
        {
            var type = request.MaintenanceType.Trim();
            if (!AllowedTypes.Contains(type))
            {
                return ServiceResult<MaintenanceRequestDto>.Fail(400, "Invalid maintenanceType.");
            }

            entity.MaintenanceType = NormalizeType(type);
        }

        if (!string.IsNullOrWhiteSpace(request.Status))
        {
            var status = request.Status.Trim();
            if (!AllowedStatuses.Contains(status))
            {
                return ServiceResult<MaintenanceRequestDto>.Fail(400, "Invalid status.");
            }

            if (status.Equals("Approved", StringComparison.OrdinalIgnoreCase) ||
                status.Equals("Rejected", StringComparison.OrdinalIgnoreCase))
            {
                return ServiceResult<MaintenanceRequestDto>.Fail(403, "Only BranchAssetAccountant can approve or reject.");
            }

            entity.Status = NormalizeStatus(status);
        }

        if (request.AccountantId.HasValue)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(403, "Operator cannot set accountantId.");
        }

        if (request.ApprovedDate.HasValue)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(403, "Operator cannot set approvedDate.");
        }

        if (request.ActualCost.HasValue)
        {
            if (request.ActualCost.Value < 0)
            {
                return ServiceResult<MaintenanceRequestDto>.Fail(400, "ActualCost must be >= 0.");
            }

            entity.ActualCost = request.ActualCost.Value;
        }

        if (request.CompletionDate.HasValue)
        {
            entity.CompletionDate = request.CompletionDate.Value;
        }

        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();

        var dto = await _repository.GetByIdAsync(id, includeDeleted: true);
        if (dto == null)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(500, "Failed to load updated maintenance request.");
        }

        return ServiceResult<MaintenanceRequestDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<MaintenanceRequestDto>> ApproveOrRejectAsync(int id, int accountantUserId, MaintenanceApprovalRequestDto request)
    {
        if (request == null)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "Request body is required.");
        }

        if (accountantUserId <= 0 || !await _repository.UserExistsAsync(accountantUserId))
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "Accountant user not found.");
        }

        var entity = await _repository.GetEntityByIdAsync(id);
        if (entity == null)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(404, "Maintenance request not found.");
        }

        var targetStatus = request.Status?.Trim();
        if (string.IsNullOrWhiteSpace(targetStatus) ||
            (!targetStatus.Equals("Approved", StringComparison.OrdinalIgnoreCase) &&
             !targetStatus.Equals("Rejected", StringComparison.OrdinalIgnoreCase)))
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(400, "Status must be 'Approved' or 'Rejected'.");
        }

        if (!string.Equals(entity.Status, "Pending", StringComparison.OrdinalIgnoreCase))
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(409, "Only Pending requests can be approved or rejected.");
        }

        entity.Status = NormalizeStatus(targetStatus);
        entity.AccountantId = accountantUserId;
        entity.ApprovedDate = request.ApprovedDate ?? DateOnly.FromDateTime(DateTime.UtcNow);
        entity.UpdatedAt = DateTime.UtcNow;

        await _repository.SaveChangesAsync();

        var dto = await _repository.GetByIdAsync(id, includeDeleted: true);
        if (dto == null)
        {
            return ServiceResult<MaintenanceRequestDto>.Fail(500, "Failed to load updated maintenance request.");
        }

        return ServiceResult<MaintenanceRequestDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var entity = await _repository.GetEntityByIdAsync(id);
        if (entity == null)
        {
            return ServiceResult<bool>.Fail(404, "Maintenance request not found.");
        }

        entity.DeletedAt = DateTime.UtcNow;
        entity.UpdatedAt = DateTime.UtcNow;
        await _repository.SaveChangesAsync();
        return ServiceResult<bool>.SuccessResult(true);
    }

    private static string NormalizeType(string type)
        => AllowedTypes.First(x => x.Equals(type, StringComparison.OrdinalIgnoreCase));

    private static string NormalizeStatus(string status)
        => AllowedStatuses.First(x => x.Equals(status, StringComparison.OrdinalIgnoreCase));
}
