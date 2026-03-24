using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Models.DTO.Vehicles;
using Models.Models;
using Service.Services.VehicleAssets.Interfaces;

namespace API.Controllers.VehicleAssets;

[ApiController]
[Authorize]
[Route("api/assets/vehicles")]
public sealed class VehicleAssetsController : ControllerBase
{
    private readonly IVehicleAssetService _service;

    public VehicleAssetsController(IVehicleAssetService service)
    {
        _service = service;
    }

    // ───────────────── Dropdown Data ─────────────────

    [HttpGet("models")]
    [ProducesResponseType(typeof(List<VehicleModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetModels()
    {
        var result = await _service.GetModelsAsync();
        return Ok(result.Data);
    }

    [HttpGet("drivers")]
    [ProducesResponseType(typeof(List<Driver>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetDrivers()
    {
        var result = await _service.GetDriversAsync();
        return Ok(result.Data);
    }

    [HttpGet("branches")]
    [ProducesResponseType(typeof(List<Branch>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetBranches()
    {
        var result = await _service.GetBranchesAsync();
        return Ok(result.Data);
    }

    // ───────────────── Asset Create ─────────────────

    [HttpPost("asset-create")]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> CreateAsset([FromBody] AssetCreateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.CreateAssetAsync(actorUserId, roles, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return StatusCode(201, result.Data);
    }

    // ───────────────── Assign / Unassign ─────────────────

    [HttpPost("{id:int}/assign")]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AssignVehicle([FromRoute] int id, [FromBody] VehicleAssignRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.AssignVehicleAsync(actorUserId, roles, id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost("{id:int}/unassign")]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UnassignVehicle([FromRoute] int id, [FromBody] VehicleUnassignRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.UnassignVehicleAsync(actorUserId, roles, id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    // ───────────────── Original Endpoints ─────────────────

    [HttpGet]
    [ProducesResponseType(typeof(List<VehicleAssetDto>), StatusCodes.Status200OK)]
    public async Task<ActionResult<List<VehicleAssetDto>>> GetVehicles(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] bool includeDeleted = false)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetVehiclesAsync(actorUserId, roles, branchId, status, includeDeleted);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpGet("branches/{branchId:int}")]
    [ProducesResponseType(typeof(List<VehicleAssetDto>), StatusCodes.Status200OK)]
    public Task<ActionResult<List<VehicleAssetDto>>> GetVehiclesByBranch(
        [FromRoute] int branchId,
        [FromQuery] string? status,
        [FromQuery] bool includeDeleted = false)
    {
        return GetVehicles(branchId, status, includeDeleted);
    }

    [HttpGet("/api/vehicles")]
    [ProducesResponseType(typeof(List<VehicleAssetDto>), StatusCodes.Status200OK)]
    public Task<ActionResult<List<VehicleAssetDto>>> GetVehiclesList(
        [FromQuery] int? branchId,
        [FromQuery] string? status,
        [FromQuery] bool includeDeleted = false)
    {
        return GetVehicles(branchId, status, includeDeleted);
    }

    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleAssetDto>> GetVehicleById([FromRoute] int id)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.GetVehicleByIdAsync(actorUserId, roles, id);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    [HttpPost]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<ActionResult<VehicleAssetDto>> CreateVehicle([FromBody] VehicleCreateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.CreateVehicleAsync(actorUserId, roles, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        var created = result.Data!;
        return CreatedAtAction(nameof(GetVehicleById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(VehicleAssetDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<VehicleAssetDto>> UpdateVehicle([FromRoute] int id, [FromBody] VehicleUpdateRequestDto request)
    {
        if (!TryGetActor(out var actorUserId, out var roles, out var errorResult))
        {
            return errorResult!;
        }

        var result = await _service.UpdateVehicleAsync(actorUserId, roles, id, request);
        if (!result.Success)
        {
            return StatusCode(result.StatusCode, new { message = result.Message });
        }

        return Ok(result.Data);
    }

    // ───────────────── Vehicle Image ─────────────────

    [HttpGet("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    public async Task<IActionResult> GetVehicleImage([FromRoute] int id)
    {
        try
        {
            var db = HttpContext.RequestServices.GetRequiredService<CarManagerContext>();
            var result = await db.Database
                .SqlQueryRaw<string>("SELECT image_url AS [Value] FROM vehicle WHERE id = {0}", id)
                .FirstOrDefaultAsync();
            return Ok(new { imageUrl = result });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[GetVehicleImage] Error: {ex}");
            return Ok(new { imageUrl = (string?)null });
        }
    }

    [HttpPost("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> UploadVehicleImage([FromRoute] int id, IFormFile file)
    {
        try
        {
            if (!TryGetActor(out _, out _, out var errorResult))
            {
                return errorResult!;
            }

            if (file == null || file.Length == 0)
            {
                return BadRequest(new { message = "No file uploaded." });
            }

            var allowedTypes = new[] { "image/jpeg", "image/png", "image/gif", "image/webp" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(new { message = "Only JPEG, PNG, GIF, and WebP images are allowed." });
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(new { message = "File size must be less than 5MB." });
            }

            // Check vehicle exists
            var db = HttpContext.RequestServices.GetRequiredService<CarManagerContext>();
            var vehicleExists = await db.Vehicles.AnyAsync(v => v.Id == id);
            if (!vehicleExists)
            {
                return NotFound(new { message = "Vehicle not found." });
            }

            // Get old image URL via raw SQL to delete the old file
            var oldImageUrl = await db.Database
                .SqlQueryRaw<string>("SELECT image_url AS [Value] FROM vehicle WHERE id = {0}", id)
                .FirstOrDefaultAsync();

            var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
            var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");

            if (!string.IsNullOrEmpty(oldImageUrl))
            {
                var oldPath = Path.Combine(webRoot, oldImageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(oldPath))
                {
                    System.IO.File.Delete(oldPath);
                }
            }

            // Save new file
            var uploadsFolder = Path.Combine(webRoot, "uploads", "vehicles");
            Directory.CreateDirectory(uploadsFolder);

            var ext = Path.GetExtension(file.FileName);
            var fileName = $"{id}_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            var imageUrl = $"/uploads/vehicles/{fileName}";

            // Update via raw SQL — completely isolated from EF Core model
            await db.Database.ExecuteSqlRawAsync(
                "UPDATE vehicle SET image_url = {0}, updated_at = GETDATE() WHERE id = {1}",
                imageUrl, id);

            return Ok(new { imageUrl });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[UploadVehicleImage] Error: {ex}");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    [HttpDelete("{id:int}/image")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> DeleteVehicleImage([FromRoute] int id)
    {
        try
        {
            if (!TryGetActor(out _, out _, out var errorResult))
            {
                return errorResult!;
            }

            var db = HttpContext.RequestServices.GetRequiredService<CarManagerContext>();
            var vehicleExists = await db.Vehicles.AnyAsync(v => v.Id == id);
            if (!vehicleExists)
            {
                return NotFound(new { message = "Vehicle not found." });
            }

            // Get current image URL
            var imageUrl = await db.Database
                .SqlQueryRaw<string>("SELECT image_url AS [Value] FROM vehicle WHERE id = {0}", id)
                .FirstOrDefaultAsync();

            if (!string.IsNullOrEmpty(imageUrl))
            {
                var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                var webRoot = env.WebRootPath ?? Path.Combine(env.ContentRootPath, "wwwroot");
                var fullPath = Path.Combine(webRoot, imageUrl.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));
                if (System.IO.File.Exists(fullPath))
                {
                    System.IO.File.Delete(fullPath);
                }
            }

            await db.Database.ExecuteSqlRawAsync(
                "UPDATE vehicle SET image_url = NULL, updated_at = GETDATE() WHERE id = {0}", id);

            return Ok(new { message = "Image deleted." });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[DeleteVehicleImage] Error: {ex}");
            return StatusCode(500, new { message = ex.Message });
        }
    }

    private bool TryGetActor(out int actorUserId, out IReadOnlyCollection<string> roles, out ActionResult? errorResult)
    {
        roles = User.FindAll(ClaimTypes.Role)
            .Select(claim => claim.Value)
            .Where(value => !string.IsNullOrWhiteSpace(value))
            .Distinct()
            .ToList();

        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue(ClaimTypes.Name)
            ?? User.FindFirstValue("sub");

        if (!int.TryParse(userIdClaim, out actorUserId))
        {
            errorResult = Unauthorized(new { message = "Invalid user identity in token." });
            return false;
        }

        if (roles.Count == 0)
        {
            errorResult = StatusCode(403, new { message = "Role is required." });
            return false;
        }

        errorResult = null;
        return true;
    }
}
