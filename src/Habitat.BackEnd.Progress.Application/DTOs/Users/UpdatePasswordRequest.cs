using System.ComponentModel.DataAnnotations;

namespace Habitat.BackEnd.Progress.Application.DTOs.Users;

public sealed class UpdatePasswordRequest
{
    [Required]
    [MaxLength(100)]
    public string CurrentPassword { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string NewPassword { get; init; } = string.Empty;
}
