using Microsoft.OpenApi.Models;

namespace Habitat.BackEnd.Progress.WebApi.Configuration;

public static class SwaggerExtensions
{
    public static IServiceCollection AddHabitatSwagger(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "Habitat: Progress API",
                Version = "1.0.0",
                Description = "API REST v1 do aplicativo mobile Habitat: Progress."
            });

            var bearerScheme = new OpenApiSecurityScheme
            {
                Name = "Authorization",
                Description = "Informe o token JWT no formato: Bearer {token}.",
                In = ParameterLocation.Header,
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "bearerAuth"
                }
            };

            options.AddSecurityDefinition("bearerAuth", bearerScheme);
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                [bearerScheme] = Array.Empty<string>()
            });
        });

        return services;
    }
}
