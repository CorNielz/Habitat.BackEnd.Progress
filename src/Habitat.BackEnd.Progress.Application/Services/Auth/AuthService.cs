using Habitat.BackEnd.Progress.Application.DTOs.Auth;
using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IPasswordHasher passwordHasher,
        ITokenService tokenService)
    {
        _userRepository = userRepository;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = request.Email.Trim().ToLowerInvariant();
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return null;
        }

        var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
        if (user is null)
        {
            return null;
        }

        var passwordMatches = _passwordHasher.Verify(user.PasswordHash, request.Password);
        if (!passwordMatches)
        {
            return null;
        }

        var (token, expiresAtUtc) = await _tokenService.GenerateTokenAsync(user, cancellationToken);

        return new LoginResponseDto
        {
            AccessToken = token,
            ExpiresAtUtc = expiresAtUtc,
            User = MapToAuthenticatedUser(user)
        };
    }

    private static AuthenticatedUserDto MapToAuthenticatedUser(User user)
    {
        return new AuthenticatedUserDto
        {
            Id = user.Id,
            Email = user.Email,
            Name = user.Name,
            Role = user.Role.ToString()
        };
    }
}
