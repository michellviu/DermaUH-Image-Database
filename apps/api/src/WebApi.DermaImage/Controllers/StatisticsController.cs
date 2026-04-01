using Application.DermaImage.DTOs;
using Application.DermaImage.Managers;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace WebApi.DermaImage.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StatisticsController : ControllerBase
{
    private readonly IStatisticsManager _manager;

    public StatisticsController(IStatisticsManager manager)
    {
        _manager = manager;
    }

    [AllowAnonymous]
    [HttpGet("overview")]
    public async Task<ActionResult<StatisticsOverviewDto>> GetOverview(CancellationToken cancellationToken)
    {
        var includePrivate = CanReadPrivateImages();
        var summary = await _manager.GetOverviewAsync(includePrivate, cancellationToken: cancellationToken);
        return Ok(summary);
    }

    private bool CanReadPrivateImages()
    {
        if (!(User.Identity?.IsAuthenticated ?? false))
        {
            return false;
        }

        return User.IsInRole("Admin") || User.IsInRole("Reviewer") || User.IsInRole("Contributor");
    }
}
