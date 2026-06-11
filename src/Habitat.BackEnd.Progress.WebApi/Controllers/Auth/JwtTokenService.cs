using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Habitat.Application.Interfaces;
using Habitat.Application.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;

namespace Habitat.BackEnd.Progress.WebApi.Features.Auth;

public class JwtTokenService : ITokenService
{
    private readonly IConfiguration _configuration;

    public JwtTokenService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public Task<(string token, DateTime expiresAt)> GenerateTokenAsync(User user, CancellationToken cancellationToken)
    {
        var issuer = _configuration["Jwt:Issuer"] ?? throw new InvalidOperationException("Jwt:Issuer not configured");
        var audience = _configuration["Jwt:Audience"] ?? throw new InvalidOperationException("Jwt:Audience not configured");

        var key = Environment.GetEnvironmentVariable("JWT_KEY");
        if (string.IsNullOrEmpty(key))
            throw new InvalidOperationException("JWT_KEY environment variable is not set.");

        var keyBytes = Encoding.UTF8.GetBytes(key);
        var securityKey = new SymmetricSecurityKey(keyBytes);
        var creds = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var expiresInMinutes = 60;
        if (int.TryParse(_configuration["Jwt:ExpiresInMinutes"], out var minutes))
            expiresInMinutes = minutes;

        var expiresAt = DateTime.UtcNow.AddMinutes(expiresInMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: expiresAt,
            signingCredentials: creds);

        var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
        return Task.FromResult((tokenString, expiresAt));
    }
}