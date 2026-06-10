using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Habitat.BackEnd.Progress.Infrastructure.Auth;

public static class JwtSigningKeyFactory
{
    private const int MinimumKeySizeInBytes = 32;

    public static SymmetricSecurityKey Create(string jwtKey)
    {
        if (string.IsNullOrWhiteSpace(jwtKey))
        {
            throw new InvalidOperationException("JWT_KEY environment variable is not set.");
        }

        var keyBytes = Encoding.UTF8.GetBytes(jwtKey);
        if (keyBytes.Length < MinimumKeySizeInBytes)
        {
            throw new InvalidOperationException($"JWT_KEY must contain at least {MinimumKeySizeInBytes} bytes.");
        }

        return new SymmetricSecurityKey(keyBytes);
    }
}
