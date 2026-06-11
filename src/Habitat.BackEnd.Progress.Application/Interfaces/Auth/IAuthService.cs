using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Auth;
using Habitat.BackEnd.Progress.Application.DTOs.Users;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<Result<UserResponse>> RegisterAsync(RegisterRequest request, CancellationToken cancellationToken = default);
    Task<Result<LoginResponse>> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
}
