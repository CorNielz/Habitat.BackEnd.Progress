using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Settings;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Services;

public interface ISettingsService
{
    Task<Result<UserSettingsResponse>> GetAsync(int userId, CancellationToken cancellationToken = default);
    Task<Result<UserSettingsResponse>> UpdateAsync(int userId, UpdateSettingsRequest request, CancellationToken cancellationToken = default);
}
