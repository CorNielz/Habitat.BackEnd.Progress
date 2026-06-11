using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Habitat.BackEnd.Progress.Infrastructure.Auth;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _options;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IOptions<JwtOptions> options)
    {
        _options = options.Value;
        ValidateOptions(_options);
        _signingKey = JwtSigningKeyFactory.Create(Environment.GetEnvironmentVariable("JWT_KEY"));
    }

    public Task<TokenResult> GenerateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var nowUtc = DateTime.UtcNow;
        var expiresAtUtc = nowUtc.AddMinutes(_options.ExpiresInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Name, user.Name),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _options.Issuer,
            audience: _options.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        return Task.FromResult(new TokenResult(new JwtSecurityTokenHandler().WriteToken(token), _options.ExpiresInMinutes * 60));
    }

    private static void ValidateOptions(JwtOptions options)
    {
        if (string.IsNullOrWhiteSpace(options.Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer configuration is required.");
        }

        if (string.IsNullOrWhiteSpace(options.Audience))
        {
            throw new InvalidOperationException("Jwt:Audience configuration is required.");
        }

        if (options.ExpiresInMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:ExpiresInMinutes must be greater than zero.");
        }
    }
}
