using Models.DTO.PendingRequests;

namespace Service.Services.Interfaces;

public interface IPendingRequestService
{
    Task<List<PendingRequestDto>> GetPendingRequestsAsync(string? status, DateTime? fromDate, DateTime? toDate);
}
