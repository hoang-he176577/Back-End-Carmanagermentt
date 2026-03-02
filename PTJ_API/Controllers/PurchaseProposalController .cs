using Microsoft.AspNetCore.Mvc;
using Models.DTO.PurchaseProposal;
using Service.Services.Interfaces;


namespace API.Controllers
{

    [Route("api/purchase-proposals")]
    public class PurchaseProposalController : BaseController
    {
        private readonly IPurchaseProposalService _service;

        public PurchaseProposalController(IPurchaseProposalService service)
        {
            _service = service;
        }

        // ==============================
        // GET ALL
        // ==============================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return HandleResult(data);
        }

        // ==============================
        // GET BY ID
        // ==============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            return HandleResult(data);
        }

        // ==============================
        // CREATE
        // ==============================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateProposalRequest request)
        {
            var userId = GetUserId();

            var proposal = await _service.CreateAsync(
                description: request.Description ?? ""
            );

            return HandleCreated(proposal, "Created successfully");
        }

        // ==============================
        // MANAGER APPROVE
        // ==============================
        [HttpPost("{id}/manager-approve")]
        public async Task<IActionResult> ManagerApprove(int id)
        {
            var userId = GetUserId();

            await _service.ApproveByManagerAsync(id, GetUserId());
            return HandleSuccess("Manager approved");
        }

        // ==============================
        // ACCOUNTANT APPROVE
        // ==============================
        [HttpPost("{id}/accountant-approve")]
        public async Task<IActionResult> AccountantApprove(int id)
        {
            var userId = GetUserId();

            await _service.ApproveByChiefAccountantAsync(id, 0);
            return HandleSuccess("Accountant approved");
        }

        // ==============================
        // REJECT
        // ==============================
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] RejectRequest request)
        {
            await _service.RejectAsync(id, request.Reason);
            return HandleSuccess("Rejected");
        }

        // ==============================
        // DELETE (soft delete)
        // ==============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            await _service.DeleteAsync(id);
            return HandleSuccess("Deleted");
        }
        // ==============================
        // GET PENDING FOR MANAGER
        // ==============================
        [HttpGet("pending")]
        public async Task<IActionResult> GetPending()
        {
            
            var data = await _service.GetPendingForManagerAsync();

      
            return HandleResult(data);
        }
        // ==============================
        // GET APPROVED LIST FOR OPERATOR (BY BRANCH)
        // ==============================
        [HttpGet("branch-approved")]
        public async Task<IActionResult> GetBranchApproved()
        {
            
            var branchId = GetBranchId();

            var data = await _service.GetApprovedByBranchAsync(branchId);
            return HandleResult(data);
        }

    }
}
