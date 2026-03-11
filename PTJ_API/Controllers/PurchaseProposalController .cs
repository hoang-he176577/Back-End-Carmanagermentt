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
        public async Task<IActionResult> Create([FromBody] CreatePurchaseProposalDto dto)
        {
            var result = await _service.CreateAsync(dto);
            return HandleCreated(result,"Create proposal successfully");
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

        // ==============================
        // GET PURCHASE PLANS
        // ==============================
        /// <summary>
        /// Lấy danh sách kế hoạch mua (approved proposals)
        /// - Manager: xem tất cả kế hoạch của tất cả chi nhánh
        /// - Operator: xem chỉ kế hoạch của chi nhánh mình
        /// </summary>
        [HttpGet("purchase-plans")]
        public async Task<IActionResult> GetPurchasePlans([FromQuery] int? branchId = null)
        {
            // Nếu không có branchId, lấy branchId của user hiện tại
            if (!branchId.HasValue || branchId.Value <= 0)
            {
                branchId = GetBranchId();
            }

            // Lấy danh sách kế hoạch mua
            // Nếu query có branchId = 0 hoặc -1, tức là xem tất cả (cho Manager)
            var plans = await _service.GetPurchasePlanAsync(branchId);
            return HandleResult(plans);
        }

    }
}
