namespace Habitat.BackEnd.Progress.Application.DTOs.Auth;

public sealed class AuthenticatedUserDto
{
    public Guid Id { get; init; }
    public string Email { get; init; } = string.Empty;
    public string? Name { get; init; }
    public string Role { get; init; } = string.Empty;
}
