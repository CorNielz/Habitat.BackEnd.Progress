using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Services.Auth;
using Microsoft.Extensions.DependencyInjection;

namespace Habitat.BackEnd.Progress.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();

        return services;
    }
}
