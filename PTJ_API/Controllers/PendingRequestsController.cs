using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Models.DTO.PendingRequests;
using Service.Services.Interfaces;

namespace API.Controllers;

[ApiController]
[Route("api/pending-requests")]
[Authorize]
public sealed class PendingRequestsController : BaseController
{
    private readonly IPendingRequestService _service;

    public PendingRequestsController(IPendingRequestService service)
    {
        _service = service;
    }

    /// <summary>
    /// Get all pending requests / proposals for Executive Management.
    /// Default status filter is "Pending". Supports date range filtering.
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(List<PendingRequestDto>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetPendingRequests(
        [FromQuery] string? status,
        [FromQuery] DateTime? fromDate,
        [FromQuery] DateTime? toDate)
    {
        var result = await _service.GetPendingRequestsAsync(status, fromDate, toDate);
        return HandleResult(result, "Pending requests retrieved successfully.");
    }
}
