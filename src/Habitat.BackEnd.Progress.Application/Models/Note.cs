namespace Habitat.BackEnd.Progress.Application.Models;

public sealed class Note
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public string? Title { get; set; }
    public string Content { get; set; } = string.Empty;
    public DateOnly Date { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
