using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.Models;

public sealed class Habit
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string? Description { get; set; }
    public HabitFrequencyType FrequencyType { get; set; }
    public string FrequencyValue { get; set; } = string.Empty;
    public DateOnly StartDate { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
