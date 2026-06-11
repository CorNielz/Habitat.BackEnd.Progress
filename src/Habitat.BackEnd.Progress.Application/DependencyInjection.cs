using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.Application.Services.Admin;
using Habitat.BackEnd.Progress.Application.Services.Auth;
using Habitat.BackEnd.Progress.Application.Services.Settings;
using Habitat.BackEnd.Progress.Application.Services.Users;
using Microsoft.Extensions.DependencyInjection;

namespace Habitat.BackEnd.Progress.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        services.AddScoped<IAuthService, AuthService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<ISettingsService, SettingsService>();
        services.AddScoped<IAdminService, AdminService>();

        return services;
    }
}
