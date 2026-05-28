namespace Habitat.Application.Models;

public class UserSettings
{
    public Guid Id { get; set; }
    public Guid UserId { get; set; }
    public string? Language { get; set; }
    public bool DarkMode { get; set; }
}
