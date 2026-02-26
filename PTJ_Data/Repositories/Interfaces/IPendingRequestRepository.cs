using Models.DTO.PendingRequests;

namespace Data.Repositories.Interfaces;

public interface IPendingRequestRepository
{
    Task<List<PendingRequestDto>> GetPendingRequestsAsync(string? status, DateTime? fromDate, DateTime? toDate);
}
