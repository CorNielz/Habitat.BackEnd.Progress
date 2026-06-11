using Habitat.BackEnd.Progress.Infrastructure.Security;

namespace UnitTests.Security;

public sealed class Pbkdf2PasswordHasherTests
{
    [Fact]
    public void Verify_ReturnsTrue_ForMatchingPassword()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("Password123!");

        Assert.True(hasher.Verify(hash, "Password123!"));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForInvalidHashFormat()
    {
        var hasher = new Pbkdf2PasswordHasher();

        Assert.False(hasher.Verify("invalid.hash", "Password123!"));
    }

    [Fact]
    public void Verify_ReturnsFalse_ForWrongPassword()
    {
        var hasher = new Pbkdf2PasswordHasher();
        var hash = hasher.Hash("Password123!");

        Assert.False(hasher.Verify(hash, "wrong-password"));
    }
}
