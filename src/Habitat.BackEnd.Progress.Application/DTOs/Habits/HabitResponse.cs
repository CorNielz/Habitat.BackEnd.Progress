using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.DTOs.Habits;

public sealed class HabitResponse
{
    public int Id { get; init; }
    public string Title { get; init; } = string.Empty;
    public string? Description { get; init; }
    public HabitFrequencyType FrequencyType { get; init; }
    public string FrequencyValue { get; init; } = string.Empty;
    public DateOnly StartDate { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
