using Data.Repositories.Interfaces;
using Models.DTO.PendingRequests;
using Service.Services.Interfaces;

namespace Service.Services.Implementations;

public sealed class PendingRequestService : IPendingRequestService
{
    private readonly IPendingRequestRepository _repository;

    public PendingRequestService(IPendingRequestRepository repository)
    {
        _repository = repository;
    }

    public Task<List<PendingRequestDto>> GetPendingRequestsAsync(
        string? status, DateTime? fromDate, DateTime? toDate)
    {
        return _repository.GetPendingRequestsAsync(status, fromDate, toDate);
    }
}
