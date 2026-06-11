using Habitat.BackEnd.Progress.Application.DTOs.Admin;
using Habitat.BackEnd.Progress.Application.DTOs.Users;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Mappings;

public static class UserMappings
{
    public static UserResponse ToUserResponse(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        CreatedAt = user.CreatedAt
    };

    public static AdminUserResponse ToAdminUserResponse(this User user) => new()
    {
        Id = user.Id,
        Name = user.Name,
        Email = user.Email,
        Role = user.Role,
        CreatedAt = user.CreatedAt,
        LastLoginAt = user.LastLoginAt
    };
}
