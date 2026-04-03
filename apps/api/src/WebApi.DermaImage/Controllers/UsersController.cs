using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Domain.DermaImage.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class UsersController : ControllerBase
{
    private readonly IUserManager _manager;

    public UsersController(IUserManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<UserResponseDto>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _manager.GetPagedAsync(page, pageSize, cancellationToken);
        var userDtos = new List<UserResponseDto>();

        foreach (var user in items)
        {
            var roles = await _manager.GetRolesAsync(user.Id, cancellationToken);
            userDtos.Add(MapToResponseDto(user, roles));
        }

        return Ok(new PagedResponse<UserResponseDto>
        {
            Items = userDtos,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<UserResponseDto>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _manager.GetByIdAsync(id, cancellationToken);
        if (user is null)
        {
            return NotFound();
        }

        var roles = await _manager.GetRolesAsync(user.Id, cancellationToken);
        return Ok(MapToResponseDto(user, roles));
    }

    [HttpPost]
    public async Task<ActionResult<UserResponseDto>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var created = await _manager.CreateAsync(dto, cancellationToken);
        var roles = await _manager.GetRolesAsync(created.Id, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToResponseDto(created, roles));
    }

    [HttpGet("{id:guid}/roles")]
    public async Task<ActionResult<IList<string>>> GetRoles(Guid id, CancellationToken cancellationToken)
    {
        var roles = await _manager.GetRolesAsync(id, cancellationToken);
        return Ok(roles);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] UpdateUserDto dto, CancellationToken cancellationToken)
    {
        await _manager.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpPost("{id:guid}/roles")]
    public async Task<IActionResult> AssignRole(Guid id, [FromBody] AssignRoleDto dto, CancellationToken cancellationToken)
    {
        await _manager.AssignRoleAsync(id, dto.Role, cancellationToken);
        return Ok();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _manager.DeleteAsync(id, cancellationToken);
        return NoContent();
    }

    private static UserResponseDto MapToResponseDto(User user, IList<string>? roles = null)
    {
        return new UserResponseDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Email = user.Email ?? string.Empty,
            Role = roles?.FirstOrDefault() ?? string.Empty,
            IsActive = user.IsActive,
            InstitutionId = user.InstitutionId,
            InstitutionName = user.Institution?.Name,
            CreatedAt = user.CreatedAt
        };
    }
}
