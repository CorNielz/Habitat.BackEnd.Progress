using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.DTOs.Dashboard;

public sealed class DashboardHistoryResponse
{
    public DashboardPeriod Period { get; init; }
    public DateOnly From { get; init; }
    public DateOnly To { get; init; }
    public IReadOnlyCollection<DashboardHistoryItem> Items { get; init; } = Array.Empty<DashboardHistoryItem>();
}
