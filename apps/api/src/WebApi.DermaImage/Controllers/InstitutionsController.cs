using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
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
    public async Task<ActionResult<IReadOnlyList<InstitutionResponseDto>>> GetAll(
        [FromQuery] bool includePrivate = false,
        CancellationToken cancellationToken = default)
    {
        var items = await _manager.GetInstitutionsAsync(includePrivate, cancellationToken);
        return Ok(items);
    }
}
