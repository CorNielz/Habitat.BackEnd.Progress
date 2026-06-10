using Habitat.Application.Interfaces;
using Habitat.Application.Models;

namespace Habitat.BackEnd.Progress.WebApi.Features.Auth;

public class InMemoryUserRepository : IUserRepository
{
    private readonly List<User> _users = new();
    public InMemoryUserRepository(IPasswordHasher hasher)
    {
        // Seed a test user: email test@local / password: Password123!
        var user = new User
        {
            Id = Guid.NewGuid(),
            Email = "test@local",
            Name = "Test User",
            Role = "Common",
            PasswordHash = hasher.Hash("Password123!")
        };
        _users.Add(user);
    }

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken)
    {
        var user = _users.FirstOrDefault(u => u.Email.Equals(email, StringComparison.OrdinalIgnoreCase));
        return Task.FromResult<User?>(user);
    }
}