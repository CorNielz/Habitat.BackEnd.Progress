using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.DTOs.Settings;

public sealed class UpdateSettingsRequest
{
    public Theme Theme { get; init; }
    public DashboardPeriod DefaultDashboardPeriod { get; init; }
    public FirstDayOfWeek FirstDayOfWeek { get; init; }
    public bool ShowHomeSummary { get; init; }
}
