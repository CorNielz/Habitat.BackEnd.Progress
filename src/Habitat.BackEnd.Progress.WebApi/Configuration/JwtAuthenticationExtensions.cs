using System.Security.Claims;
using System.Text.Json;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Infrastructure.Auth;
using Habitat.BackEnd.Progress.WebApi.ProblemDetails;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace Habitat.BackEnd.Progress.WebApi.Configuration;

public static class JwtAuthenticationExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtOptions = configuration.GetSection(JwtOptions.SectionName).Get<JwtOptions>()
            ?? throw new InvalidOperationException("Jwt configuration section is required.");

        var signingKey = JwtSigningKeyFactory.Create(Environment.GetEnvironmentVariable("JWT_KEY"));

        services
            .AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
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
                options.Events = new JwtBearerEvents
                {
                    OnChallenge = async context =>
                    {
                        context.HandleResponse();
                        await WriteProblemAsync(context.HttpContext, StatusCodes.Status401Unauthorized, "Unauthorized", "Authentication is required to access this resource.");
                    },
                    OnForbidden = async context =>
                    {
                        await WriteProblemAsync(context.HttpContext, StatusCodes.Status403Forbidden, "Forbidden", "You do not have permission to access this resource.");
                    }
                };
            });

        services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireAuthenticatedUser().RequireRole(UserRole.ADMIN.ToString()));
            options.AddPolicy("UserOrAdmin", policy => policy.RequireAuthenticatedUser().RequireRole(UserRole.USER.ToString(), UserRole.ADMIN.ToString()));
        });

        return services;
    }

    private static async Task WriteProblemAsync(HttpContext context, int status, string title, string detail)
    {
        if (context.Response.HasStarted)
        {
            return;
        }

        context.Response.StatusCode = status;
        context.Response.ContentType = "application/problem+json";
        var problem = ProblemDetailsFactory.Create(context, status, title, detail);
        await JsonSerializer.SerializeAsync(context.Response.Body, problem, new JsonSerializerOptions(JsonSerializerDefaults.Web), context.RequestAborted);
    }
}
