using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Users;
using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.Application.Mappings;

namespace Habitat.BackEnd.Progress.Application.Services.Users;

public sealed class UserService : IUserService
{
    private readonly IUserRepository _users;
    private readonly IPasswordHasher _passwordHasher;

    public UserService(IUserRepository users, IPasswordHasher passwordHasher)
    {
        _users = users;
        _passwordHasher = passwordHasher;
    }

    public async Task<Result<UserResponse>> GetProfileAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is null || !user.IsActive
            ? Result<UserResponse>.NotFound("users.not_found", "The authenticated user was not found.")
            : Result<UserResponse>.Success(user.ToUserResponse());
    }

    public async Task<Result<UserResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var name = request.Name.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            return Result<UserResponse>.Validation("users.invalid_name", "Name is required.");
        }

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result<UserResponse>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        await _users.UpdateProfileAsync(userId, name, DateTime.UtcNow, cancellationToken);
        user.Name = name;
        return Result<UserResponse>.Success(user.ToUserResponse());
    }

    public async Task<Result> UpdatePasswordAsync(int userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (string.IsNullOrWhiteSpace(request.CurrentPassword) || string.IsNullOrWhiteSpace(request.NewPassword))
        {
            return Result.Validation("users.invalid_password", "Current password and new password are required.");
        }

        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null || !user.IsActive)
        {
            return Result.NotFound("users.not_found", "The authenticated user was not found.");
        }

        if (!_passwordHasher.Verify(user.PasswordHash, request.CurrentPassword))
        {
            return Result.Unauthorized("users.invalid_current_password", "Current password is invalid.");
        }

        var newHash = _passwordHasher.Hash(request.NewPassword);
        await _users.UpdatePasswordAsync(userId, newHash, DateTime.UtcNow, cancellationToken);
        return Result.Success();
    }
}
