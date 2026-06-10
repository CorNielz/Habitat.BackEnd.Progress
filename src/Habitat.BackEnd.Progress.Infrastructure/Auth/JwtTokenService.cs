using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Habitat.BackEnd.Progress.Infrastructure.Auth;

public sealed class JwtTokenService : ITokenService
{
    private readonly JwtOptions _jwtOptions;
    private readonly SymmetricSecurityKey _signingKey;

    public JwtTokenService(IOptions<JwtOptions> jwtOptions)
    {
        _jwtOptions = jwtOptions.Value;

        if (string.IsNullOrWhiteSpace(_jwtOptions.Issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer configuration is required.");
        }

        if (string.IsNullOrWhiteSpace(_jwtOptions.Audience))
        {
            throw new InvalidOperationException("Jwt:Audience configuration is required.");
        }

        if (_jwtOptions.ExpiresInMinutes <= 0)
        {
            throw new InvalidOperationException("Jwt:ExpiresInMinutes must be greater than zero.");
        }

        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
        _signingKey = JwtSigningKeyFactory.Create(jwtKey!);
    }

    public Task<(string Token, DateTime ExpiresAtUtc)> GenerateTokenAsync(User user, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(user);

        var nowUtc = DateTime.UtcNow;
        var expiresAtUtc = nowUtc.AddMinutes(_jwtOptions.ExpiresInMinutes);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(ClaimTypes.NameIdentifier, user.Id.ToString()),
            new(ClaimTypes.Email, user.Email),
            new(ClaimTypes.Role, user.Role.ToString())
        };

        if (!string.IsNullOrWhiteSpace(user.Name))
        {
            claims.Add(new Claim(ClaimTypes.Name, user.Name));
        }

        var credentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256);
        var token = new JwtSecurityToken(
            issuer: _jwtOptions.Issuer,
            audience: _jwtOptions.Audience,
            claims: claims,
            notBefore: nowUtc,
            expires: expiresAtUtc,
            signingCredentials: credentials);

        var accessToken = new JwtSecurityTokenHandler().WriteToken(token);

        return Task.FromResult((accessToken, expiresAtUtc));
    }
}
