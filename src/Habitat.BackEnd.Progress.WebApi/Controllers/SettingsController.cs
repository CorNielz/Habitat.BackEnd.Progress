using Habitat.BackEnd.Progress.Application.DTOs.Settings;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.WebApi.Extensions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Habitat.BackEnd.Progress.WebApi.Controllers;

[ApiController]
[Authorize]
[Route("api/v1/settings")]
public sealed class SettingsController : ControllerBase
{
    private readonly ISettingsService _settings;

    public SettingsController(ISettingsService settings)
    {
        _settings = settings;
    }

    [HttpGet]
    [ProducesResponseType(typeof(UserSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Get(CancellationToken cancellationToken)
    {
        var result = await _settings.GetAsync(User.GetUserId(), cancellationToken);
        return result.ToActionResult(this);
    }

    [HttpPut]
    [ProducesResponseType(typeof(UserSettingsResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Update([FromBody] UpdateSettingsRequest request, CancellationToken cancellationToken)
    {
        var result = await _settings.UpdateAsync(User.GetUserId(), request, cancellationToken);
        return result.ToActionResult(this);
    }
}
