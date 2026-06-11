using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default);
    Task<bool> EmailExistsAsync(string email, int? excludedUserId = null, CancellationToken cancellationToken = default);
    Task<User> CreateAsync(User user, UserSettings settings, CancellationToken cancellationToken = default);
    Task UpdateProfileAsync(int userId, string name, DateTime updatedAtUtc, CancellationToken cancellationToken = default);
    Task UpdatePasswordAsync(int userId, string passwordHash, DateTime updatedAtUtc, CancellationToken cancellationToken = default);
    Task UpdateLastLoginAsync(int userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken = default);
    Task<PagedResponse<User>> ListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task UpdateRoleAsync(int userId, Enums.UserRole role, DateTime updatedAtUtc, CancellationToken cancellationToken = default);
}
