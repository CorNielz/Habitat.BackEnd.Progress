using Habitat.BackEnd.Progress.Application.DTOs.Auth;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Services.Auth;
using Habitat.BackEnd.Progress.Infrastructure.Security;
using Habitat.BackEnd.Progress.TestSupport;

namespace UnitTests.Auth;

public sealed class AuthServiceTests
{
    [Fact]
    public async Task RegisterAsync_CreatesUserWithUserRole()
    {
        var store = new InMemoryHabitatStore();
        var service = new AuthService(store, new Pbkdf2PasswordHasher(), new FixedTokenService());

        var result = await service.RegisterAsync(new RegisterRequest { Name = "Daniel", Email = "daniel@email.com", Password = "Senha@123" });

        Assert.True(result.IsSuccess);
        Assert.Equal(UserRole.USER, result.Value!.Role);
        Assert.Equal("daniel@email.com", result.Value.Email);
    }

    [Fact]
    public async Task RegisterAsync_ReturnsConflict_WhenEmailAlreadyExists()
    {
        var store = new InMemoryHabitatStore();
        var service = new AuthService(store, new Pbkdf2PasswordHasher(), new FixedTokenService());
        await service.RegisterAsync(new RegisterRequest { Name = "Daniel", Email = "daniel@email.com", Password = "Senha@123" });

        var result = await service.RegisterAsync(new RegisterRequest { Name = "Daniel", Email = "daniel@email.com", Password = "Senha@123" });

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.email_already_exists", result.Error!.Code);
    }

    [Fact]
    public async Task LoginAsync_ReturnsToken_WhenCredentialsAreValid()
    {
        var store = new InMemoryHabitatStore();
        var hasher = new Pbkdf2PasswordHasher();
        var service = new AuthService(store, hasher, new FixedTokenService());
        await service.RegisterAsync(new RegisterRequest { Name = "Daniel", Email = "daniel@email.com", Password = "Senha@123" });

        var result = await service.LoginAsync(new LoginRequest { Email = "daniel@email.com", Password = "Senha@123" });

        Assert.True(result.IsSuccess);
        Assert.Equal("fixed-token", result.Value!.AccessToken);
        Assert.Equal(3600, result.Value.ExpiresIn);
    }

    [Fact]
    public async Task LoginAsync_ReturnsUnauthorized_WhenPasswordIsInvalid()
    {
        var store = new InMemoryHabitatStore();
        var hasher = new Pbkdf2PasswordHasher();
        var service = new AuthService(store, hasher, new FixedTokenService());
        await service.RegisterAsync(new RegisterRequest { Name = "Daniel", Email = "daniel@email.com", Password = "Senha@123" });

        var result = await service.LoginAsync(new LoginRequest { Email = "daniel@email.com", Password = "wrong" });

        Assert.False(result.IsSuccess);
        Assert.Equal("auth.invalid_credentials", result.Error!.Code);
    }
}
