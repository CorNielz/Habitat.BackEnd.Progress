namespace Habitat.BackEnd.Progress.Application.Models;

public sealed class HabitRecord
{
    public int Id { get; set; }
    public int HabitId { get; set; }
    public DateOnly RecordDate { get; set; }
    public bool Completed { get; set; } = true;
    public string? Note { get; set; }
    public DateTime RecordedAt { get; set; }
}
