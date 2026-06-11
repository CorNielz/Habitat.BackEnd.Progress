using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Admin;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Services;

public interface IAdminService
{
    Task<Result<PagedResponse<AdminUserResponse>>> ListUsersAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<Result<AdminUserResponse>> GetUserByIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result> UpdateUserRoleAsync(int userId, UpdateUserRoleRequest request, CancellationToken cancellationToken = default);
}
