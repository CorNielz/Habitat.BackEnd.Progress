using Dapper;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Infrastructure.Database;

namespace Habitat.BackEnd.Progress.Infrastructure.Persistence;

public sealed class UserSettingsRepository : IUserSettingsRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDatabaseRetryPolicy _retry;

    public UserSettingsRepository(IDbConnectionFactory connectionFactory, IDatabaseRetryPolicy retry)
    {
        _connectionFactory = connectionFactory;
        _retry = retry;
    }

    public Task<UserSettings?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            SELECT id, user_id AS UserId, theme AS Theme, default_dashboard_period AS DefaultDashboardPeriod,
                   first_day_of_week AS FirstDayOfWeek, show_home_summary AS ShowHomeSummary, updated_at AS UpdatedAt
            FROM user_settings
            WHERE user_id = @UserId;
            """;
        var row = await connection.QuerySingleOrDefaultAsync<UserSettingsRow>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
        return row?.ToSettings();
    }, cancellationToken);

    public Task<UserSettings> UpsertAsync(UserSettings settings, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            INSERT INTO user_settings (user_id, theme, default_dashboard_period, first_day_of_week, show_home_summary, updated_at)
            VALUES (@UserId, @Theme, @DefaultDashboardPeriod, @FirstDayOfWeek, @ShowHomeSummary, @UpdatedAt)
            ON DUPLICATE KEY UPDATE
                theme = VALUES(theme),
                default_dashboard_period = VALUES(default_dashboard_period),
                first_day_of_week = VALUES(first_day_of_week),
                show_home_summary = VALUES(show_home_summary),
                updated_at = VALUES(updated_at);
            SELECT id, user_id AS UserId, theme AS Theme, default_dashboard_period AS DefaultDashboardPeriod,
                   first_day_of_week AS FirstDayOfWeek, show_home_summary AS ShowHomeSummary, updated_at AS UpdatedAt
            FROM user_settings
            WHERE user_id = @UserId;
            """;

        var row = await connection.QuerySingleAsync<UserSettingsRow>(new CommandDefinition(sql, new
        {
            settings.UserId,
            Theme = settings.Theme.ToString(),
            DefaultDashboardPeriod = settings.DefaultDashboardPeriod.ToString(),
            FirstDayOfWeek = settings.FirstDayOfWeek.ToString(),
            settings.ShowHomeSummary,
            settings.UpdatedAt
        }, cancellationToken: ct));

        return row.ToSettings();
    }, cancellationToken);

    private sealed class UserSettingsRow
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public string Theme { get; init; } = string.Empty;
        public string DefaultDashboardPeriod { get; init; } = string.Empty;
        public string FirstDayOfWeek { get; init; } = string.Empty;
        public bool ShowHomeSummary { get; init; }
        public DateTime UpdatedAt { get; init; }

        public UserSettings ToSettings() => new()
        {
            Id = Id,
            UserId = UserId,
            Theme = Enum.Parse<Theme>(Theme, ignoreCase: true),
            DefaultDashboardPeriod = Enum.Parse<DashboardPeriod>(DefaultDashboardPeriod, ignoreCase: true),
            FirstDayOfWeek = Enum.Parse<FirstDayOfWeek>(FirstDayOfWeek, ignoreCase: true),
            ShowHomeSummary = ShowHomeSummary,
            UpdatedAt = UpdatedAt
        };
    }
}
