using Habitat.BackEnd.Progress.Application.DTOs.Users;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Application.Services.Users;
using Habitat.BackEnd.Progress.Infrastructure.Security;
using Habitat.BackEnd.Progress.TestSupport;

namespace UnitTests.Users;

public sealed class UserServiceTests
{
    [Fact]
    public async Task UpdateProfileAsync_UpdatesOnlyAuthenticatedUser()
    {
        var store = CreateStore();
        var service = new UserService(store, new Pbkdf2PasswordHasher());

        var result = await service.UpdateProfileAsync(1, new UpdateProfileRequest { Name = "Daniel Atualizado" });

        Assert.True(result.IsSuccess);
        Assert.Equal("Daniel Atualizado", result.Value!.Name);
    }

    [Fact]
    public async Task UpdatePasswordAsync_ReturnsUnauthorized_WhenCurrentPasswordIsWrong()
    {
        var store = CreateStore();
        var service = new UserService(store, new Pbkdf2PasswordHasher());

        var result = await service.UpdatePasswordAsync(1, new UpdatePasswordRequest { CurrentPassword = "wrong", NewPassword = "NovaSenha@123" });

        Assert.False(result.IsSuccess);
        Assert.Equal("users.invalid_current_password", result.Error!.Code);
    }

    private static InMemoryHabitatStore CreateStore()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var store = new InMemoryHabitatStore();
        store.Users.Add(new User { Id = 1, Name = "Daniel", Email = "daniel@email.com", PasswordHash = hasher.Hash("Senha@123"), Role = UserRole.USER, IsActive = true, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow });
        return store;
    }
}
