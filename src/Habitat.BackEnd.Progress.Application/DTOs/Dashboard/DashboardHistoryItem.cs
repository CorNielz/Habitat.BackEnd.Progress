namespace Habitat.BackEnd.Progress.Application.DTOs.Dashboard;

public sealed class DashboardHistoryItem
{
    public DateOnly Date { get; init; }
    public int CompletedHabits { get; init; }
    public double CompletionRate { get; init; }
}
