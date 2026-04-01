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

    public InstitutionsController(IInstitutionManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<InstitutionResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _manager.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(new PagedResponse<InstitutionResponseDto>
        {
            Items = items.Select(MapToResponseDto),
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
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

    private bool IsAdmin()
    {
        var roleClaims = User.FindAll(ClaimTypes.Role)
            .Select(c => c.Value)
            .Concat(User.FindAll("role").Select(c => c.Value))
            .Concat(User.FindAll("roles").Select(c => c.Value))
            .Concat(User.FindAll("http://schemas.microsoft.com/ws/2008/06/identity/claims/role").Select(c => c.Value));

        return roleClaims.Any(value => string.Equals(value, "Admin", StringComparison.OrdinalIgnoreCase));
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
