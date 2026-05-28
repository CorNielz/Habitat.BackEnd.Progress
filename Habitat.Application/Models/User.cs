namespace Habitat.Application.Models;

public class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = null!;
    public string PasswordHash { get; set; } = null!;
    public string? Name { get; set; }
    public UserRole Role { get; set; }
    public UserSettings Settings { get; set; } = null!;
}

public enum UserRole
{
    Common = 0,
    Admin = 1
}
