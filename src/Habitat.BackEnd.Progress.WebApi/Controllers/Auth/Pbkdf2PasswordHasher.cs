using System.Security.Cryptography;
using System.Text;
using Habitat.Application.Interfaces;

namespace Habitat.BackEnd.Progress.WebApi.Features.Auth;

public class Pbkdf2PasswordHasher : IPasswordHasher
{
    private const int SaltSize = 16;
    private const int HashSize = 32;
    private const int Iterations = 100_000;

    public string Hash(string password)
    {
        using var rng = RandomNumberGenerator.Create();
        var salt = new byte[SaltSize];
        rng.GetBytes(salt);

        using var pbkdf2 = new Rfc2898DeriveBytes(password, salt, Iterations, HashAlgorithmName.SHA256);
        var hash = pbkdf2.GetBytes(HashSize);

        return $"{Iterations}.{Convert.ToBase64String(salt)}.{Convert.ToBase64String(hash)}";
    }

    public bool Verify(string hashedPassword, string providedPassword)
    {
        var parts = hashedPassword.Split('.');
        if (parts.Length != 3)
            return false;

        var iterations = int.Parse(parts[0]);
        var salt = Convert.FromBase64String(parts[1]);
        var hash = Convert.FromBase64String(parts[2]);

        using var pbkdf2 = new Rfc2898DeriveBytes(providedPassword, salt, iterations, HashAlgorithmName.SHA256);
        var computed = pbkdf2.GetBytes(hash.Length);

        return CryptographicOperations.FixedTimeEquals(computed, hash);
    }
}