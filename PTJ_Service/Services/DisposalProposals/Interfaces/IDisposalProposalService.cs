using Models.DTO.DisposalProposals;
using Service.Services.Common;

namespace Service.Services.DisposalProposals.Interfaces;

public interface IDisposalProposalService
{
    Task<ServiceResult<DisposalProposalPagedResultDto>> GetListAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        DisposalProposalListQueryDto query);

    Task<ServiceResult<DisposalProposalDto>> GetByIdAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int id);

    Task<ServiceResult<List<DisposalProposalDto>>> GetByVehicleIdAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        int vehicleId);

    Task<ServiceResult<DisposalProposalDto>> CreateAsync(
        int actorUserId,
        IReadOnlyCollection<string> roles,
        DisposalProposalCreateRequestDto request);

    Task<ServiceResult<DisposalProposalDto>> ApproveAsync(
        int proposalId,
        int managerUserId,
        DisposalProposalApproveRequestDto? request);

    Task<ServiceResult<DisposalProposalDto>> RejectAsync(
        int proposalId,
        int managerUserId,
        DisposalProposalRejectRequestDto? request);
}
