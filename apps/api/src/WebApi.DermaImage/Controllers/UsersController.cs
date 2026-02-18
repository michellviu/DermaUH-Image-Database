using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Domain.DermaImage.Entities;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserManager _manager;

    public UsersController(IUserManager manager)
    {
        _manager = manager;
    }

    [HttpGet]
    public async Task<ActionResult<PagedResponse<User>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _manager.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(new PagedResponse<User>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<User>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var user = await _manager.GetByIdAsync(id, cancellationToken);
        return user is null ? NotFound() : Ok(user);
    }

    [HttpPost]
    public async Task<ActionResult<User>> Create([FromBody] CreateUserDto dto, CancellationToken cancellationToken)
    {
        var created = await _manager.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpGet("{id:guid}/roles")]
    public async Task<ActionResult<IList<string>>> GetRoles(Guid id, CancellationToken cancellationToken)
    {
        var roles = await _manager.GetRolesAsync(id, cancellationToken);
        return Ok(roles);
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
}
