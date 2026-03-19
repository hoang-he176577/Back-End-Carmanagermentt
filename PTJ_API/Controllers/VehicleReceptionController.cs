using Microsoft.AspNetCore.Mvc;
using Models.DTO.PurchaseProposal;
using Service.Services.Interfaces;

namespace API.Controllers
{
    [Route("api/vehicle-receptions")]
    public class VehicleReceptionController : BaseController
    {
        private readonly IVehicleReceptionService _service;

        public VehicleReceptionController(IVehicleReceptionService service)
        {
            _service = service;
        }

        // ==============================
        // GET BY ID
        // ==============================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var data = await _service.GetByIdAsync(id);
            if (data == null)
                return NotFound();

            return HandleResult(data);
        }

        // ==============================
        // GET BY PROPOSAL ID
        // ==============================
        [HttpGet("proposal/{proposalId}")]
        public async Task<IActionResult> GetByProposalId(int proposalId)
        {
            var data = await _service.GetByProposalIdAsync(proposalId);
            return HandleResult(data);
        }

        // ==============================
        // GET BY BRANCH ID
        // ==============================
        [HttpGet("branch/{branchId}")]
        public async Task<IActionResult> GetByBranchId(int branchId)
        {
            var data = await _service.GetByBranchIdAsync(branchId);
            return HandleResult(data);
        }

        // ==============================
        // GET PENDING BY BRANCH ID
        // ==============================
        [HttpGet("branch/{branchId}/pending")]
        public async Task<IActionResult> GetPendingByBranch(int branchId)
        {
            var data = await _service.GetPendingByBranchAsync(branchId);
            return HandleResult(data);
        }

        // ==============================
        // GET ALL (Manager only)
        // ==============================
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var data = await _service.GetAllAsync();
            return HandleResult(data);
        }

        // ==============================
        // CREATE
        // ==============================
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateVehicleReceptionDto dto)
        {
            // Lấy userId từ claim (Operator tạo bản ghi)
            var userId = GetUserId();
            
            // Lấy RequestedDate từ request header hoặc body
            var requestedDate = DateOnly.FromDateTime(DateTime.Now);

            var result = await _service.CreateAsync(dto, userId, requestedDate);
            return HandleCreated(result, "Reception record created successfully");
        }

        // ==============================
        // UPDATE
        // ==============================
        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] CreateVehicleReceptionDto dto)
        {
            var result = await _service.UpdateAsync(id, dto);
            return HandleResult(result, "Reception record updated successfully");
        }

        // ==============================
        // COMPLETE
        // ==============================
        [HttpPost("{id}/complete")]
        public async Task<IActionResult> Complete(int id)
        {
            var result = await _service.CompleteAsync(id);
            return HandleResult(result, "Reception record completed");
        }

        // ==============================
        // REJECT
        // ==============================
        [HttpPost("{id}/reject")]
        public async Task<IActionResult> Reject(int id, [FromBody] UpdateVehicleReceptionStatusDto dto)
        {
            var result = await _service.RejectAsync(id, dto.Reason);
            return HandleResult(result, "Reception record rejected");
        }

        // ==============================
        // DELETE
        // ==============================
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var result = await _service.DeleteAsync(id);
            if (!result)
                return NotFound();

            return HandleSuccess("Reception record deleted");
        }
    }
}
