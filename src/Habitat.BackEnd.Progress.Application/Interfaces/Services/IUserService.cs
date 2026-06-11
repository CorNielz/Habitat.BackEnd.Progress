using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Users;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Services;

public interface IUserService
{
    Task<Result<UserResponse>> GetProfileAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<UserResponse>> UpdateProfileAsync(int userId, UpdateProfileRequest request, CancellationToken cancellationToken = default);
    Task<Result> UpdatePasswordAsync(int userId, UpdatePasswordRequest request, CancellationToken cancellationToken = default);
}
