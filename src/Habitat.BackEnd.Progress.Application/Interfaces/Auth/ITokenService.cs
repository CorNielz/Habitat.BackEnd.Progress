using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Auth;

public interface ITokenService
{
    Task<(string Token, DateTime ExpiresAtUtc)> GenerateTokenAsync(User user, CancellationToken cancellationToken = default);
}
