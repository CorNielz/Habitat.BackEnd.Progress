using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Settings;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.Application.Mappings;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Services.Settings;

public sealed class SettingsService : ISettingsService
{
    private readonly IUserSettingsRepository _settings;
    private readonly IUserRepository _users;

    public SettingsService(IUserSettingsRepository settings, IUserRepository users)
    {
        _settings = settings;
        _users = users;
    }

    public async Task<Result<UserSettingsResponse>> GetAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<UserSettingsResponse>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        var settings = await _settings.GetByUserIdAsync(userId, cancellationToken);
        if (settings is null)
        {
            settings = await _settings.UpsertAsync(new UserSettings { UserId = userId, UpdatedAt = DateTime.UtcNow }, cancellationToken);
        }

        return Result<UserSettingsResponse>.Success(settings.ToResponse());
    }

    public async Task<Result<UserSettingsResponse>> UpdateAsync(int userId, UpdateSettingsRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<UserSettingsResponse>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        var settings = new UserSettings
        {
            UserId = userId,
            Theme = request.Theme,
            DefaultDashboardPeriod = request.DefaultDashboardPeriod,
            FirstDayOfWeek = request.FirstDayOfWeek,
            ShowHomeSummary = request.ShowHomeSummary,
            UpdatedAt = DateTime.UtcNow
        };

        var saved = await _settings.UpsertAsync(settings, cancellationToken);
        return Result<UserSettingsResponse>.Success(saved.ToResponse());
    }
}
