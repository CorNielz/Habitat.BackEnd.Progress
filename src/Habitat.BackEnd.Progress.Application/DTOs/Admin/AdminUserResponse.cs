using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.DTOs.Admin;

public sealed class AdminUserResponse
{
    public int Id { get; init; }
    public string Name { get; init; } = string.Empty;
    public string Email { get; init; } = string.Empty;
    public UserRole Role { get; init; }
    public DateTime CreatedAt { get; init; }
    public DateTime? LastLoginAt { get; init; }
}
