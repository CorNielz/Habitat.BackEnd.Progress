using Habitat.BackEnd.Progress.Application.Interfaces.Auth;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Infrastructure.Persistence.InMemory;

public sealed class InMemoryUserRepository : IUserRepository
{
    private readonly IReadOnlyCollection<User> _users;

    public InMemoryUserRepository(IPasswordHasher passwordHasher)
    {
        ArgumentNullException.ThrowIfNull(passwordHasher);

        _users = new[]
        {
            new User
            {
                Id = Guid.Parse("0a75c546-7de6-46cc-9540-187e126458e3"),
                Email = "test@local",
                Name = "Habitat Test User",
                PasswordHash = passwordHasher.Hash("Password123!"),
                Role = UserRole.Common,
                Settings = new UserSettings
                {
                    Id = Guid.Parse("05305987-ef25-4c9d-b608-0a6cb6eb117b"),
                    UserId = Guid.Parse("0a75c546-7de6-46cc-9540-187e126458e3"),
                    Language = "pt-BR",
                    DarkMode = false
                }
            },
            new User
            {
                Id = Guid.Parse("88d0cf63-c702-45f5-a5e1-3a52e65b719b"),
                Email = "admin@local",
                Name = "Habitat Admin User",
                PasswordHash = passwordHasher.Hash("Admin123!"),
                Role = UserRole.Admin,
                Settings = new UserSettings
                {
                    Id = Guid.Parse("cd8518dd-b82f-48b1-a44d-94f0c80af189"),
                    UserId = Guid.Parse("88d0cf63-c702-45f5-a5e1-3a52e65b719b"),
                    Language = "pt-BR",
                    DarkMode = false
                }
            }
        };
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return Task.FromResult<User?>(null);
        }

        var normalizedEmail = email.Trim().ToLowerInvariant();
        var user = _users.FirstOrDefault(current => current.Email.Equals(normalizedEmail, StringComparison.OrdinalIgnoreCase));

        return Task.FromResult(user);
    }

    public Task<User?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        var user = _users.FirstOrDefault(current => current.Id == id);

        return Task.FromResult(user);
    }
}
