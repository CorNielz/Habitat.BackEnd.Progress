using Habitat.BackEnd.Progress.Application.DTOs.Users;

namespace Habitat.BackEnd.Progress.Application.DTOs.Auth;

public sealed class LoginResponse
{
    public string AccessToken { get; init; } = string.Empty;
    public string TokenType { get; init; } = "Bearer";
    public int ExpiresIn { get; init; }
    public UserResponse User { get; init; } = new();
}
