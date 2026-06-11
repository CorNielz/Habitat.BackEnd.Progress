using Dapper;
using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Infrastructure.Auth;
using Habitat.BackEnd.Progress.Infrastructure.Database;
using Habitat.BackEnd.Progress.Infrastructure.Persistence;
using Habitat.BackEnd.Progress.Infrastructure.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Habitat.BackEnd.Progress.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        SqlMapper.Settings.CommandTimeout = 30;
        SqlMapper.AddTypeHandler(new DateOnlyTypeHandler());
        DefaultTypeMap.MatchNamesWithUnderscores = true;

        services.Configure<JwtOptions>(configuration.GetSection(JwtOptions.SectionName));
        services.Configure<DatabaseRetryOptions>(configuration.GetSection(DatabaseRetryOptions.SectionName));

        services.AddSingleton<IDbConnectionFactory, MySqlConnectionFactory>();
        services.AddSingleton<IDatabaseRetryPolicy, MySqlRetryPolicy>();

        services.AddSingleton<IPasswordHasher, Pbkdf2PasswordHasher>();
        services.AddScoped<ITokenService, JwtTokenService>();

        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserSettingsRepository, UserSettingsRepository>();
        services.AddScoped<IHabitRepository, HabitRepository>();
        services.AddScoped<IHabitRecordRepository, HabitRecordRepository>();
        services.AddScoped<INoteRepository, NoteRepository>();

        return services;
    }
}
