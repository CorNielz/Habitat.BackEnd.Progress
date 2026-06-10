namespace Habitat.BackEnd.Progress.Application.DTOs.Auth;

public sealed class LoginResponseDto
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public DateTime ExpiresAtUtc { get; init; }
    public AuthenticatedUserDto User { get; init; } = new();
}
