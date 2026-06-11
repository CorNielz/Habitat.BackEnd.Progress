using System.ComponentModel.DataAnnotations;

namespace Habitat.BackEnd.Progress.Application.DTOs.HabitRecords;

public sealed class CreateHabitRecordRequest
{
    public DateOnly RecordDate { get; init; }

    [MaxLength(1000)]
    public string? Note { get; init; }
}
