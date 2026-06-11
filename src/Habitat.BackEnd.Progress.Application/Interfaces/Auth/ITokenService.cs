using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Auth;

public interface ITokenService
{
    Task<TokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
}

public sealed record TokenResult(string AccessToken, int ExpiresIn);
