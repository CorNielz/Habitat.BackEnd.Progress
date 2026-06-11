using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.HabitRecords;

namespace Habitat.BackEnd.Progress.Application.Interfaces.Services;

public interface IHabitRecordService
{
    Task<Result<PagedResponse<HabitRecordResponse>>> ListAsync(int userId, int habitId, PaginationRequest pagination, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default);
    Task<Result<HabitRecordResponse>> CreateAsync(int userId, int habitId, CreateHabitRecordRequest request, CancellationToken cancellationToken = default);
    Task<Result> DeleteByDateAsync(int userId, int habitId, DateOnly recordDate, CancellationToken cancellationToken = default);
}
