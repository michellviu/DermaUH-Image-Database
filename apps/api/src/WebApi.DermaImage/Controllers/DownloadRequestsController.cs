using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/download-requests")]
public class DownloadRequestsController : ControllerBase
{
    private readonly IDownloadManager _manager;

    public DownloadRequestsController(IDownloadManager manager)
    {
        _manager = manager;
    }

    [HttpPost]
    [Authorize]
    public async Task<IActionResult> Create(
        [FromBody] CreateDownloadRequestDto dto,
        CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var result = await _manager.CreateRequestAsync(userId.Value, dto, ct);
        return Ok(result);
    }

    [HttpGet("pending")]
    [Authorize(Roles = "Admin")]
    public async Task<ActionResult<PagedResponse<DownloadRequestResponseDto>>> GetPending(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        CancellationToken ct = default)
    {
        var (items, totalCount) = await _manager.GetPendingRequestsAsync(page, pageSize, ct);
        return Ok(new PagedResponse<DownloadRequestResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpPost("{id:guid}/review")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Review(
        Guid id,
        [FromBody] ReviewDownloadRequestDto dto,
        CancellationToken ct)
    {
        var adminId = GetCurrentUserId();
        if (adminId is null) return Unauthorized();

        await _manager.ReviewRequestAsync(id, adminId.Value, dto, ct);
        return NoContent();
    }

    [HttpGet("my")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<DownloadRequestResponseDto>>> GetMyRequests(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var requests = await _manager.GetRequestsByUserAsync(userId.Value, ct);
        return Ok(requests);
    }

    [HttpGet("authorization")]
    [Authorize]
    public async Task<ActionResult<object>> CheckAuthorization(CancellationToken ct)
    {
        var userId = GetCurrentUserId();
        if (userId is null) return Unauthorized();

        var isAuthorized = await _manager.HasActiveAuthorizationAsync(userId.Value, ct);
        return Ok(new { isAuthorized });
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub");
        return Guid.TryParse(value, out var id) ? id : null;
    }
}
