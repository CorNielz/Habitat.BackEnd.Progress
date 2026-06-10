namespace Habitat.BackEnd.Progress.Application.Models;

public sealed class User
{
    public Guid Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string? Name { get; set; }
    public UserRole Role { get; set; } = UserRole.Common;
    public UserSettings? Settings { get; set; }
}
