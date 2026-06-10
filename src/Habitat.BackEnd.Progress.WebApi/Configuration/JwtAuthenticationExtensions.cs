using System.Security.Claims;
using Habitat.BackEnd.Progress.Infrastructure.Auth;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Habitat.BackEnd.Progress.WebApi.Configuration;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = ReadJwtOptions(configuration);
        var jwtKey = Environment.GetEnvironmentVariable("JWT_KEY");
        var signingKey = JwtSigningKeyFactory.Create(jwtKey!);

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = true;
                options.SaveToken = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidateAudience = true,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    ValidIssuer = jwtOptions.Issuer,
                    ValidAudience = jwtOptions.Audience,
                    IssuerSigningKey = signingKey,
                    ClockSkew = TimeSpan.FromMinutes(1),
                    NameClaimType = ClaimTypes.Name,
                    RoleClaimType = ClaimTypes.Role
                };
            });

        return services;
    }

    private static JwtOptions ReadJwtOptions(IConfiguration configuration)
    {
        var issuer = configuration["Jwt:Issuer"];
        if (string.IsNullOrWhiteSpace(issuer))
        {
            throw new InvalidOperationException("Jwt:Issuer configuration is required.");
        }

        var audience = configuration["Jwt:Audience"];
        if (string.IsNullOrWhiteSpace(audience))
        {
            throw new InvalidOperationException("Jwt:Audience configuration is required.");
        }

        var expiresInMinutesValue = configuration["Jwt:ExpiresInMinutes"];
        var expiresInMinutes = 60;
        if (!string.IsNullOrWhiteSpace(expiresInMinutesValue)
            && (!int.TryParse(expiresInMinutesValue, out expiresInMinutes) || expiresInMinutes <= 0))
        {
            throw new InvalidOperationException("Jwt:ExpiresInMinutes must be a positive integer.");
        }

        return new JwtOptions
        {
            Issuer = issuer,
            Audience = audience,
            ExpiresInMinutes = expiresInMinutes
        };
    }
}
