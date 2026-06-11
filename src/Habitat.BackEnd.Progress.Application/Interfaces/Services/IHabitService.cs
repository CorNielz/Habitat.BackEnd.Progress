using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Habits;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Services;

public interface IHabitService
{
    Task<Result<PagedResponse<HabitResponse>>> ListAsync(int userId, PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<Result<HabitResponse>> GetByIdAsync(int userId, int habitId, CancellationToken cancellationToken = default);
    Task<Result<HabitResponse>> CreateAsync(int userId, CreateHabitRequest request, CancellationToken cancellationToken = default);
    Task<Result<HabitResponse>> UpdateAsync(int userId, int habitId, UpdateHabitRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteAsync(int userId, int habitId, CancellationToken cancellationToken = default);
}
