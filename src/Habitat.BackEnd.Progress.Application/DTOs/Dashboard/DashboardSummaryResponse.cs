using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.DTOs.Dashboard;

public sealed class DashboardSummaryResponse
{
    public DashboardPeriod Period { get; init; }
    public int TotalHabits { get; init; }
    public int CompletedToday { get; init; }
    public double CompletionRate { get; init; }
    public int CurrentStreak { get; init; }
    public int NotesCount { get; init; }
}
