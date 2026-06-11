namespace Habitat.BackEnd.Progress.Application.Interfaces.Auth;

public interface IPasswordHasher
{
    string Hash(string password);
    bool Verify(string hashedPassword, string providedPassword);
}
