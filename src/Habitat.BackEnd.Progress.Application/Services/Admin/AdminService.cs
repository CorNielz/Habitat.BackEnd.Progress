using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Admin;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.Application.Mappings;

namespace Habitat.BackEnd.Progress.Application.Services.Admin;

public sealed class AdminService : IAdminService
{
    private readonly IUserRepository _users;

    public AdminService(IUserRepository users)
    {
        _users = users;
    }

    public async Task<Result<PagedResponse<AdminUserResponse>>> ListUsersAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var page = await _users.ListAsync(pagination, cancellationToken);
        return Result<PagedResponse<AdminUserResponse>>.Success(PagedResponse<AdminUserResponse>.Create(
            page.Items.Select(u => u.ToAdminUserResponse()).ToArray(),
            page.TotalItems,
            pagination));
    }

    public async Task<Result<AdminUserResponse>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is null
            ? Result<AdminUserResponse>.NotFound("users.not_found", "The requested user was not found.")
            : Result<AdminUserResponse>.Success(user.ToAdminUserResponse());
    }

    public async Task<Result> UpdateUserRoleAsync(int userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        if (user is null)
        {
            return Result.NotFound("users.not_found", "The requested user was not found.");
        }

        await _users.UpdateRoleAsync(userId, request.Role, DateTime.UtcNow, cancellationToken);
        return Result.Success();
    }
}
