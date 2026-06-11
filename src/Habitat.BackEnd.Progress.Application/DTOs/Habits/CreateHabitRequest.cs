using System.ComponentModel.DataAnnotations;
using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.DTOs.Habits;

public sealed class CreateHabitRequest
{
    [Required]
    [MinLength(1)]
    [MaxLength(120)]
    public string Title { get; init; } = string.Empty;

    [MaxLength(1000)]
    public string? Description { get; init; }

    public HabitFrequencyType FrequencyType { get; init; }

    [Required]
    [MaxLength(100)]
    public string FrequencyValue { get; init; } = string.Empty;

    public DateOnly StartDate { get; init; }
}
