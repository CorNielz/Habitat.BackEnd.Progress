using Habitat.BackEnd.Progress.Application.Enums;

namespace Habitat.BackEnd.Progress.Application.DTOs.Admin;

public sealed class UpdateUserRoleRequest
{
    public UserRole Role { get; init; }
}
