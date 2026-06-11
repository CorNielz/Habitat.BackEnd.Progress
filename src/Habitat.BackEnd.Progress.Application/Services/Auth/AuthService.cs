using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Auth;
using Habitat.BackEnd.Progress.Application.DTOs.Users;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Mappings;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Services.Auth;

public sealed class AuthService : IAuthService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;
    private readonly ITokenService _tokenService;

    public AuthService(IUserRepository users, IPasswordHasher passwordHasher, ITokenService tokenService)
    {
        _users = users;
        _passwordHasher = passwordHasher;
        _tokenService = tokenService;
    }

    public async Task<Result<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name.Trim();
        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<UserResponse>.Validation("auth.invalid_registration", "Name, email and password are required.");
        }

        if (await _users.EmailExistsAsync(email, cancellationToken: cancellationToken))
        {
            return Result<UserResponse>.Conflict("auth.email_already_exists", "A user with this e-mail already exists.");
        }

        var now = DateTime.UtcNow;
        var user = new User
        {
            Name = name,
            Email = email,
            PasswordHash = _passwordHasher.Hash(request.Password),
            Role = UserRole.USER,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var settings = new UserSettings
        {
            Theme = Theme.SYSTEM,
            DefaultDashboardPeriod = DashboardPeriod.MONTH,
            FirstDayOfWeek = FirstDayOfWeek.MONDAY,
            ShowHomeSummary = true,
            UpdatedAt = now
        };

        var created = await _users.CreateAsync(user, settings, cancellationToken);
        return Result<UserResponse>.Success(created.ToUserResponse());
    }

    public async Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var email = NormalizeEmail(request.Email);
        if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(request.Password))
        {
            return Result<LoginResponse>.Unauthorized("auth.invalid_credentials", "Invalid email or password.");
        }

        var user = await _users.GetByEmailAsync(email, cancellationToken);
        if (user is null || !user.IsActive || !_passwordHasher.Verify(user.PasswordHash, request.Password))
        {
            return Result<LoginResponse>.Unauthorized("auth.invalid_credentials", "Invalid email or password.");
        }

        var token = await _tokenService.GenerateTokenAsync(user, cancellationToken);
        await _users.UpdateLastLoginAsync(user.Id, DateTime.UtcNow, cancellationToken);

        return Result<LoginResponse>.Success(new LoginResponse
        {
            AccessToken = token.AccessToken,
            ExpiresIn = token.ExpiresIn,
            User = user.ToUserResponse()
        });
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();
}
