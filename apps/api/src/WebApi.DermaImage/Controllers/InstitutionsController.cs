using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Domain.DermaImage.Entities;
using Microsoft.AspNetCore.Mvc;

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
    public async Task<ActionResult<PagedResponse<Institution>>> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        var (items, totalCount) = await _manager.GetPagedAsync(page, pageSize, cancellationToken);
        return Ok(new PagedResponse<Institution>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize
        });
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<Institution>> GetById(Guid id, CancellationToken cancellationToken)
    {
        var institution = await _manager.GetByIdAsync(id, cancellationToken);
        return institution is null ? NotFound() : Ok(institution);
    }

    [HttpPost]
    public async Task<ActionResult<Institution>> Create([FromBody] CreateInstitutionDto dto, CancellationToken cancellationToken)
    {
        var created = await _manager.CreateAsync(dto, cancellationToken);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:guid}")]
    public async Task<IActionResult> Update(Guid id, [FromBody] CreateInstitutionDto dto, CancellationToken cancellationToken)
    {
        await _manager.UpdateAsync(id, dto, cancellationToken);
        return NoContent();
    }

    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
    {
        await _manager.DeleteAsync(id, cancellationToken);
        return NoContent();
    }
}
