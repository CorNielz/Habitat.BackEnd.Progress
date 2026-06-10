using System.ComponentModel.DataAnnotations;

namespace Habitat.BackEnd.Progress.Application.DTOs.Auth;

public sealed class LoginRequestDto
{
    [Required]
    [EmailAddress]
    [MaxLength(254)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(128)]
    public string Password { get; init; } = string.Empty;
}
