using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Repositories;

public interface IUserSettingsRepository
{
    Task<UserSettings?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default);
    Task<UserSettings> UpsertAsync(UserSettings settings, CancellationToken cancellationToken = default);
}
