using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Infrastructure.Security;
using Habitat.BackEnd.Progress.TestSupport;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace FunctionalTests;

public sealed class HabitatWebApplicationFactory : WebApplicationFactory<Program>
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Environment.SetEnvironmentVariable("JWT_KEY", "functional-tests-habitat-secret-key-32bytes-minimum");

        builder.UseEnvironment("Development");
        builder.ConfigureServices(services =>
        {
            services.RemoveAll<IUserRepository>();
            services.RemoveAll<IUserSettingsRepository>();
            services.RemoveAll<IHabitRepository>();
            services.RemoveAll<IHabitRecordRepository>();
            services.RemoveAll<INoteRepository>();

            var store = SeedStore();
            services.AddSingleton(store);
            services.AddSingleton<IUserRepository>(store);
            services.AddSingleton<IUserSettingsRepository>(store);
            services.AddSingleton<IHabitRepository>(store);
            services.AddSingleton<IHabitRecordRepository>(store);
            services.AddSingleton<INoteRepository>(store);
        });
    }

    private static InMemoryHabitatStore SeedStore()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var now = DateTime.UtcNow;
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User
        {
            Id = 1,
            Role = UserRole.USER,
            RoleId = 1,
            Name = "Habitat Test User",
            Email = "test@local",
            PasswordHash = hasher.Hash("Password123!"),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        store.Users.Add(new User
        {
            Id = 2,
            Role = UserRole.ADMIN,
            RoleId = 2,
            Name = "Habitat Admin User",
            Email = "admin@local",
            PasswordHash = hasher.Hash("Admin123!"),
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        });
        return store;
    }
}
