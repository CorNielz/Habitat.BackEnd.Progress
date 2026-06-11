using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Repositories;

public interface IHabitRecordRepository
{
    Task<PagedResponse<HabitRecord>> ListByHabitAsync(int habitId, PaginationRequest pagination, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<HabitRecord>> ListByUserBetweenAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default);
    Task<int> CountByUserAndDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default);
    Task<bool> ExistsAsync(int habitId, DateOnly recordDate, CancellationToken cancellationToken = default);
    Task<HabitRecord> CreateAsync(HabitRecord record, CancellationToken cancellationToken = default);
    Task<bool> DeleteByDateAsync(int habitId, DateOnly recordDate, CancellationToken cancellationToken = default);
}
