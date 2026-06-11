using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.HabitRecords;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.Application.Mappings;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Services.HabitRecords;

public sealed class HabitRecordService : IHabitRecordService
{
    private readonly IHabitRepository _habits;
    private readonly IHabitRecordRepository _records;

    public HabitRecordService(IHabitRepository habits, IHabitRecordRepository records)
    {
        _habits = habits;
        _records = records;
    }

    public async Task<Result<PagedResponse<HabitRecordResponse>>> ListAsync(int userId, int habitId, PaginationRequest pagination, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var habit = await _habits.GetByIdForUserAsync(userId, habitId, cancellationToken);
        if (habit is null)
        {
            return Result<PagedResponse<HabitRecordResponse>>.NotFound("habits.not_found", "The requested habit was not found.");
        }

        if (from.HasValue && to.HasValue && from > to)
        {
            return Result<PagedResponse<HabitRecordResponse>>.Validation("records.invalid_date_range", "The 'from' date cannot be greater than the 'to' date.");
        }

        var page = await _records.ListByHabitAsync(habitId, pagination, from, to, cancellationToken);
        return Result<PagedResponse<HabitRecordResponse>>.Success(PagedResponse<HabitRecordResponse>.Create(
            page.Items.Select(r => r.ToResponse()).ToArray(),
            page.TotalItems,
            pagination));
    }

    public async Task<Result<HabitRecordResponse>> CreateAsync(int userId, int habitId, CreateHabitRecordRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var habit = await _habits.GetByIdForUserAsync(userId, habitId, cancellationToken);
        if (habit is null)
        {
            return Result<HabitRecordResponse>.NotFound("habits.not_found", "The requested habit was not found.");
        }

        if (request.RecordDate == default)
        {
            return Result<HabitRecordResponse>.Validation("records.invalid_record_date", "Record date is required.");
        }

        if (await _records.ExistsAsync(habitId, request.RecordDate, cancellationToken))
        {
            return Result<HabitRecordResponse>.Conflict("records.already_exists", "This habit already has a completion record for the informed date.");
        }

        var record = new HabitRecord
        {
            HabitId = habitId,
            RecordDate = request.RecordDate,
            Note = string.IsNullOrWhiteSpace(request.Note) ? null : request.Note.Trim(),
            Completed = true,
            RecordedAt = DateTime.UtcNow
        };

        var created = await _records.CreateAsync(record, cancellationToken);
        return Result<HabitRecordResponse>.Success(created.ToResponse());
    }

    public async Task<Result> DeleteByDateAsync(int userId, int habitId, DateOnly recordDate, CancellationToken cancellationToken = default)
    {
        var habit = await _habits.GetByIdForUserAsync(userId, habitId, cancellationToken);
        if (habit is null)
        {
            return Result.NotFound("habits.not_found", "The requested habit was not found.");
        }

        if (recordDate == default)
        {
            return Result.Validation("records.invalid_record_date", "Record date is required.");
        }

        var deleted = await _records.DeleteByDateAsync(habitId, recordDate, cancellationToken);
        return deleted
            ? Result.Success()
            : Result.NotFound("records.not_found", "The requested completion record was not found.");
    }
}
