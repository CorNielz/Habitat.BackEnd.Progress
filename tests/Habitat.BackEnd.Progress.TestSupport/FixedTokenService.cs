using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.TestSupport;

public sealed class FixedTokenService : ITokenService
{
    public Task<TokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(new TokenResult("fixed-token", 3600));
    }
}
