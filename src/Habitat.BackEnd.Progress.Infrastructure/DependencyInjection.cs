using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Infrastructure.Auth;
using Habitat.BackEnd.Progress.Infrastructure.Persistence.InMemory;
using Habitat.BackEnd.Progress.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Habitat.BackEnd.Progress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddSingleton<ITokenService, JwtTokenService>();

        services.AddSingleton<IUserRepository, InMemoryUserRepository>();

        return services;
    }
}
