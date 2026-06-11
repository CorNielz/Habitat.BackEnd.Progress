namespace Habitat.BackEnd.Progress.Application.DTOs.Notes;

public sealed class NoteResponse
{
    public int Id { get; init; }
    public string? Title { get; init; }
    public string Content { get; init; } = string.Empty;
    public DateOnly Date { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime UpdatedAt { get; init; }
}
