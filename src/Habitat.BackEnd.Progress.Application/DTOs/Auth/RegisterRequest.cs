using System.ComponentModel.DataAnnotations;

namespace Habitat.BackEnd.Progress.Application.DTOs.Auth;

public sealed class RegisterRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(120)]
    public string Name { get; init; } = string.Empty;

    [Required]
    [EmailAddress]
    [MaxLength(180)]
    public string Email { get; init; } = string.Empty;

    [Required]
    [MinLength(8)]
    [MaxLength(100)]
    public string Password { get; init; } = string.Empty;
}
