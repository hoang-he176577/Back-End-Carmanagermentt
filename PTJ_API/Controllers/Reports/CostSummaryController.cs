using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Service.Services.Reports.Interfaces;

namespace API.Controllers.Reports
{
    [ApiController]
    [Route("api/reports")]
    public sealed class CostSummaryController : ControllerBase
    {
        private readonly ICostSummaryService _service;

        public CostSummaryController(ICostSummaryService service)
        {
            _service = service;
        }

        [HttpGet("estimated-costs")]
        [Authorize(Roles = "Branch Asset Accountant,Chief Accountant,Executive Management")]
        public async Task<IActionResult> GetEstimatedCosts([FromQuery] int? branchId = null)
        {
            var result = await _service.GetEstimatedCostsAsync(branchId);
            return Ok(result);
        }

        [HttpGet("estimated-costs/branches")]
        [Authorize(Roles = "Branch Asset Accountant,Chief Accountant,Executive Management")]
        public async Task<IActionResult> GetEstimatedCostsByBranch()
        {
            var result = await _service.GetEstimatedCostsByBranchAsync();
            return Ok(result);
        }
    }
}
