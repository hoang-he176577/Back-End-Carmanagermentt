using Data.Repositories.DriverTransfer.Interfaces;
using Microsoft.EntityFrameworkCore;
using Models.DTO.DriverTransfer;
using Models.Models;
using Service.Services.Common;
using Service.Services.DriverTransfer.Interfaces;

namespace Service.Services.DriverTransfer.Implementations;

public sealed class DriverTransferService : IDriverTransferService
{
    private readonly IDriverTransferRepository _repository;
    private readonly CarManagerContext _context;

    public DriverTransferService(IDriverTransferRepository repository, CarManagerContext context)
    {
        _repository = repository;
        _context = context;
    }

    public Task<ServiceResult<List<DriverTransferRequestResponseDto>>> GetAllAsync(int actorUserId, IReadOnlyCollection<string> roles, string? status)
    {
        if (!IsOperator(roles))
            return Task.FromResult(ServiceResult<List<DriverTransferRequestResponseDto>>.Fail(403, "Only Operator can access driver transfer requests."));

        return GetAllCoreAsync(status);
    }

    private async Task<ServiceResult<List<DriverTransferRequestResponseDto>>> GetAllCoreAsync(string? status)
    {
        var data = await _repository.GetAllRequestsAsync(status);
        return ServiceResult<List<DriverTransferRequestResponseDto>>.SuccessResult(data);
    }

    public async Task<ServiceResult<DriverTransferRequestResponseDto>> GetByIdAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        if (!IsOperator(roles))
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(403, "Only Operator can access driver transfer requests.");

        var dto = await _repository.GetRequestByIdAsync(id);
        return dto == null
            ? ServiceResult<DriverTransferRequestResponseDto>.Fail(404, "Driver transfer request not found.")
            : ServiceResult<DriverTransferRequestResponseDto>.SuccessResult(dto);
    }

    public async Task<ServiceResult<DriverTransferRequestResponseDto>> CreateAsync(int actorUserId, IReadOnlyCollection<string> roles, CreateDriverTransferRequestDto request)
    {
        if (!IsOperator(roles))
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(403, "Only Operator can create driver transfer requests.");
        if (request == null)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "Request body is required.");
        if (request.RequestedQuantity <= 0)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "RequestedQuantity must be greater than 0.");

        var branchId = await _repository.GetUserBranchIdAsync(actorUserId);
        if (!branchId.HasValue)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "User is not assigned to a branch.");
        if (!await _repository.BranchExistsAsync(branchId.Value))
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "Requesting branch not found.");

        var created = await _repository.CreateRequestAsync(new DriverTransferRequest
        {
            RequestingBranchId = branchId.Value,
            RequestedQuantity = request.RequestedQuantity,
            FulfilledQuantity = 0,
            Status = "Pending",
            Reason = request.Reason?.Trim(),
            CreatedByUserId = actorUserId
        });

        var dto = await _repository.GetRequestByIdAsync(created.Id);
        return ServiceResult<DriverTransferRequestResponseDto>.SuccessResult(dto!, 201);
    }

    public async Task<ServiceResult<DriverTransferRequestResponseDto>> ConfirmAsync(int actorUserId, IReadOnlyCollection<string> roles, int id, ConfirmDriverTransferDto request)
    {
        if (!IsOperator(roles))
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(403, "Only Operator can confirm transfers.");
        if (request == null || request.DriverIds == null || request.DriverIds.Count == 0)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "DriverIds is required.");

        var transfer = await _repository.GetRequestEntityByIdAsync(id);
        if (transfer == null)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(404, "Driver transfer request not found.");
        if (!string.Equals(transfer.Status, "Pending", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(transfer.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "Only Pending/InProgress requests can be confirmed.");

        var operatorBranchId = await _repository.GetUserBranchIdAsync(actorUserId);
        if (!operatorBranchId.HasValue)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "User is not assigned to a branch.");
        if (operatorBranchId.Value == transfer.RequestingBranchId)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(403, "You cannot confirm transfer for your own branch request.");

        var distinctIds = request.DriverIds.Distinct().ToList();
        var drivers = await _repository.GetDriverEntitiesByIdsAsync(distinctIds);
        if (drivers.Count != distinctIds.Count)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "One or more drivers do not exist.");

        foreach (var driver in drivers)
        {
            if (driver.BranchId != operatorBranchId.Value)
                return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, $"Driver {driver.Id} is not in your branch.");

            var hasVehicle = await _context.Vehicles.AnyAsync(v => v.CurrentDriverId == driver.Id && v.DeletedAt == null);
            if (hasVehicle)
                return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, $"Driver {driver.Id} is currently assigned to a vehicle.");
        }

        var now = DateTime.Now;
        var details = new List<DriverTransferDetail>();
        foreach (var driver in drivers)
        {
            details.Add(new DriverTransferDetail
            {
                TransferRequestId = transfer.Id,
                DriverId = driver.Id,
                FromBranchId = operatorBranchId.Value,
                ConfirmedByUserId = actorUserId,
                TransferDate = now,
                CreatedAt = now
            });
            driver.BranchId = transfer.RequestingBranchId;
            driver.UpdatedAt = now;
        }

        await _repository.AddTransferDetailsAsync(details);
        transfer.FulfilledQuantity += drivers.Count;
        transfer.Status = transfer.FulfilledQuantity >= transfer.RequestedQuantity ? "Completed" : "InProgress";
        transfer.UpdatedAt = now;
        await _repository.UpdateRequestAsync(transfer);

        var dto = await _repository.GetRequestByIdAsync(transfer.Id);
        return ServiceResult<DriverTransferRequestResponseDto>.SuccessResult(dto!);
    }

    public async Task<ServiceResult<DriverTransferRequestResponseDto>> CancelAsync(int actorUserId, IReadOnlyCollection<string> roles, int id)
    {
        var isExecutive = IsExecutive(roles);
        var isOperator = IsOperator(roles);
        if (!isExecutive && !isOperator)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(403, "You do not have permission to cancel requests.");

        var transfer = await _repository.GetRequestEntityByIdAsync(id);
        if (transfer == null)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(404, "Driver transfer request not found.");

        if (!string.Equals(transfer.Status, "Pending", StringComparison.OrdinalIgnoreCase)
            && !string.Equals(transfer.Status, "InProgress", StringComparison.OrdinalIgnoreCase))
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(400, "Only Pending/InProgress requests can be cancelled.");

        if (!isExecutive && transfer.CreatedByUserId != actorUserId)
            return ServiceResult<DriverTransferRequestResponseDto>.Fail(403, "Only request creator or Executive Management can cancel.");

        transfer.Status = "Cancelled";
        transfer.UpdatedAt = DateTime.Now;
        await _repository.UpdateRequestAsync(transfer);

        var dto = await _repository.GetRequestByIdAsync(transfer.Id);
        return ServiceResult<DriverTransferRequestResponseDto>.SuccessResult(dto!);
    }

    private static bool IsOperator(IReadOnlyCollection<string> roles)
        => roles.Any(r => string.Equals(r, "Operator", StringComparison.OrdinalIgnoreCase));

    private static bool IsExecutive(IReadOnlyCollection<string> roles)
        => roles.Any(r => string.Equals(r, "Executive Management", StringComparison.OrdinalIgnoreCase));
}
