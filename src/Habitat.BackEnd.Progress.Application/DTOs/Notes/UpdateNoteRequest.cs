using System.ComponentModel.DataAnnotations;

namespace Habitat.BackEnd.Progress.Application.DTOs.Notes;

public sealed class UpdateNoteRequest
{
    [MaxLength(150)]
    public string? Title { get; init; }

    [Required]
    [MinLength(1)]
    public string Content { get; init; } = string.Empty;

    public DateOnly Date { get; init; }
}
