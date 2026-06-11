using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.Models;

public sealed class UserSettings
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public Theme Theme { get; set; } = Theme.SYSTEM;
    public DashboardPeriod DefaultDashboardPeriod { get; set; } = DashboardPeriod.MONTH;
    public FirstDayOfWeek FirstDayOfWeek { get; set; } = FirstDayOfWeek.MONDAY;
    public bool ShowHomeSummary { get; set; } = true;
    public DateTime UpdatedAt { get; set; }
}
