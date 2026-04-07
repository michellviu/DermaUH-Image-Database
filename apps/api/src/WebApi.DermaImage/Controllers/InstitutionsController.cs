using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Domain.DermaImage.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class InstitutionsController : ControllerBase
{
    private readonly IInstitutionManager _manager;
    private readonly IInstitutionMembershipManager _membershipManager;

    public InstitutionsController(IInstitutionManager manager, IInstitutionMembershipManager membershipManager)
    {
        _manager = manager;
        _membershipManager = membershipManager;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<InstitutionResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);
        var (items, totalCount) = await _manager.GetPagedAsync(validPage, validPageSize, cancellationToken);
        return Ok(new PagedResponse<InstitutionResponseDto>
        {
            Items = items.Select(MapToResponseDto),
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<InstitutionResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var institution = await _manager.GetByIdAsync(id, cancellationToken);
        return institution is null ? NotFound() : Ok(MapToResponseDto(institution));
    }

    [HttpPost]
    [Authorize]
    public async Task<ActionResult<InstitutionResponseDto>> Create([FromBody] CreateInstitutionDto dto, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var created = await _manager.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponseDto(created));
    }

    [HttpPut("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateInstitutionDto dto, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        await _manager.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    [Authorize]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        await _manager.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    [HttpGet("{id:guid}/responsibles")]
    [Authorize]
    public async Task<ActionResult<IReadOnlyList<InstitutionResponsibleResponseDto>>> GetResponsibles(Guid id, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var responsibles = await _membershipManager.GetInstitutionResponsiblesAsync(id, cancellationToken);
        return Ok(responsibles);
    }

    [HttpPost("{id:guid}/responsibles")]
    [Authorize]
    public async Task<IActionResult> AssignResponsible(Guid id, [FromBody] AssignInstitutionResponsibleDto dto, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        var actorUserId = GetCurrentUserId();
        if (actorUserId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _membershipManager.AddResponsibleAsync(id, dto.UserId, actorUserId.Value, cancellationToken);
            return Ok();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpDelete("{id:guid}/responsibles/{userId:guid}")]
    [Authorize]
    public async Task<IActionResult> RemoveResponsible(Guid id, Guid userId, CancellationToken cancellationToken)
    {
        if (!IsAdmin())
        {
            return Forbid();
        }

        try
        {
            await _membershipManager.RemoveResponsibleAsync(id, userId, cancellationToken);
            return Ok();
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("join-requests")]
    [Authorize]
    public async Task<ActionResult<InstitutionJoinRequestResponseDto>> CreateJoinRequest([FromBody] CreateInstitutionJoinRequestDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var request = await _membershipManager.CreateJoinRequestAsync(userId.Value, dto, cancellationToken);
            return Ok(request);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("join-requests/mine")]
    [Authorize]
    public async Task<ActionResult<PagedResponse<InstitutionJoinRequestResponseDto>>> GetMyJoinRequests(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);
        var (items, totalCount) = await _membershipManager.GetMyJoinRequestsAsync(userId.Value, validPage, validPageSize, cancellationToken);
        return Ok(new PagedResponse<InstitutionJoinRequestResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize,
        });
    }

    [HttpPost("leave")]
    [Authorize]
    public async Task<IActionResult> LeaveMyInstitution(CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            await _membershipManager.LeaveInstitutionAsync(userId.Value, cancellationToken);
            return Ok(new { message = "Has salido de la institución correctamente." });
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("join-requests/inbox")]
    [Authorize]
    public async Task<ActionResult<PagedResponse<InstitutionJoinRequestResponseDto>>> GetResponsibleInbox(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 5,
        CancellationToken cancellationToken = default)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        var validPage = Math.Max(1, page);
        var validPageSize = Math.Clamp(pageSize, 1, 100);
        var (items, totalCount) = await _membershipManager.GetResponsibleInboxAsync(userId.Value, validPage, validPageSize, cancellationToken);
        return Ok(new PagedResponse<InstitutionJoinRequestResponseDto>
        {
            Items = items,
            TotalCount = totalCount,
            Page = validPage,
            PageSize = validPageSize,
        });
    }

    [HttpPost("join-requests/{requestId:guid}/review")]
    [Authorize]
    public async Task<ActionResult<InstitutionJoinRequestResponseDto>> ReviewJoinRequest(Guid requestId, [FromBody] ReviewInstitutionJoinRequestDto dto, CancellationToken cancellationToken)
    {
        var userId = GetCurrentUserId();
        if (userId is null)
        {
            return Unauthorized();
        }

        try
        {
            var result = await _membershipManager.ReviewJoinRequestAsync(userId.Value, requestId, dto, cancellationToken);
            return Ok(result);
        }
        catch (UnauthorizedAccessException)
        {
            return Forbid();
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    private bool IsAdmin()
    {
        var roleClaims = User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Concat(User.FindAll("role").Select(c => c.Value))
            .Concat(User.FindAll("roles").Select(c => c.Value))
            .Concat(User.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value));

        return roleClaims.Any(value => string.Equals(value, "Admin", StringComparison.OrdinalIgnoreCase));
    }

    private Guid? GetCurrentUserId()
    {
        var value = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return Guid.TryParse(value, out var id) ? id : null;
    }

    private static InstitutionResponseDto MapToResponseDto(Institution institution)
    {
        return new InstitutionResponseDto
        {
            Id = institution.Id,
            Name = institution.Name,
            Description = institution.Description,
            Country = institution.Country,
            City = institution.City,
            Address = institution.Address,
            Website = institution.Website,
            ContactEmail = institution.ContactEmail,
            LogoUrl = institution.LogoUrl,
            CreatedAt = institution.CreatedAt
        };
    }
}
