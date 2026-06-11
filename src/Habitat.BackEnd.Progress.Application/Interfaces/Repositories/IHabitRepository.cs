using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Repositories;

public interface IHabitRepository
{
    Task<PagedResponse<Habit>> ListByUserAsync(int userId, PaginationRequest pagination, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<Habit>> ListActiveByUserAsync(int userId, CancellationToken cancellationToken = default);
    Task<Habit?> GetByIdForUserAsync(int userId, int habitId, CancellationToken cancellationToken = default);
    Task<Habit> CreateAsync(Habit habit, CancellationToken cancellationToken = default);
    Task<bool> UpdateAsync(Habit habit, CancellationToken cancellationToken = default);
    Task<bool> DeleteAsync(int userId, int habitId, CancellationToken cancellationToken = default);
}
