using Data.Repositories.Interfaces;
using Models.DTO.PurchaseProposal;
using Models.Models;
using Service.Services.Interfaces;

namespace Service.Services.Implementations
{
    public class PostPurchaseService : IPostPurchaseService
    {
        private readonly IPurchaseProposalRepository _proposalRepository;
        private readonly CarManagerContext _context;

        public PostPurchaseService(IPurchaseProposalRepository proposalRepository, CarManagerContext context)
        {
            _proposalRepository = proposalRepository;
            _context = context;
        }

        public async Task ConfirmReceptionAsync(AssetReceptionRequest request, int operatorId)
        {
            var proposal = await _proposalRepository.GetByIdAsync(request.ProposalId)
                           ?? throw new Exception("Purchase proposal not found.");

            if (proposal.Status != "Approved")
                throw new Exception("Proposal must be approved before reception.");

            // Update status to pending payment
            proposal.Status = "Received_Pending_Payment";
            proposal.Description += $"\n[Reception Confirmed]: License Plate {request.LicensePlate} by User {operatorId}";

            _proposalRepository.Update(proposal);
            await _proposalRepository.SaveChangesAsync();
        }

        public async Task ConfirmPaymentAndActivateAssetAsync(int proposalId, int accountantId)
        {
            var proposal = await _proposalRepository.GetByIdAsync(proposalId)
                           ?? throw new Exception("Purchase proposal not found.");

            // 1. Complete the proposal workflow
            proposal.Status = "Completed";
            proposal.ApproveByChiefAccountant(accountantId);

            // 2. Automatically create and activate the new Vehicle asset
            var detail = proposal.BulkPurchaseDetails.FirstOrDefault();
            var newVehicle = new Vehicle
            {
                LicensePlate = "PENDING_UPDATE", // To be updated with real plate after registration
                Status = "Active",
                CurrentBranchId = detail?.BranchId,
                OriginalCost = proposal.ProposedCost ?? 0,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now
            };

            await _context.Vehicles.AddAsync(newVehicle);
            _proposalRepository.Update(proposal);
            await _proposalRepository.SaveChangesAsync();
        }
    }
}