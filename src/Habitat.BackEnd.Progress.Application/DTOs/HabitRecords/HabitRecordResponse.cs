namespace Habitat.BackEnd.Progress.Application.DTOs.HabitRecords;

public sealed class HabitRecordResponse
{
    public int Id { get; init; }
    public int HabitId { get; init; }
    public DateOnly RecordDate { get; init; }
    public string? Note { get; init; }
    public DateTime RecordedAt { get; init; }
}
