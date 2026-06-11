using System.ComponentModel.DataAnnotations;

namespace Habitat.BackEnd.Progress.Application.DTOs.Users;

public sealed class UpdateProfileRequest
{
    [Required]
    [MinLength(2)]
    [MaxLength(120)]
    public string Name { get; init; } = string.Empty;
}
