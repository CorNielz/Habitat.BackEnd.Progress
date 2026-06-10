using Habitat.BackEnd.Progress.Application.DTOs.Auth;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Auth;

public interface IAuthService
{
    Task<LoginResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
}
