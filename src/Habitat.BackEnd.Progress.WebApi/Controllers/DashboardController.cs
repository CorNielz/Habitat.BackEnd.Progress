using Habitat.BackEnd.Progress.Application.DTOs.Dashboard;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Habitat.BackEnd.Progress.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/dashboard")]
public sealed class DashboardController : ControllerBase
{
    private readonly IDashboardService _dashboard;

    public DashboardController(IDashboardService dashboard)
    {
        _dashboard = dashboard;
    }

    [HttpGet]
    [ProducesResponseType(typeof(DashboardSummaryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Summary([FromQuery] DashboardPeriod period = DashboardPeriod.MONTH, CancellationToken cancellationToken = default)
    {
        var result = await _dashboard.GetSummaryAsync(User.GetUserId(), period, cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpGet("history")]
    [ProducesResponseType(typeof(DashboardHistoryResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> History([FromQuery] DashboardPeriod period = DashboardPeriod.MONTH, [FromQuery] DateOnly? from = null, [FromQuery] DateOnly? to = null, CancellationToken cancellationToken = default)
    {
        var result = await _dashboard.GetHistoryAsync(User.GetUserId(), period, from, to, cancellationToken);
        return result.ToActionResult(this);
    }
}
