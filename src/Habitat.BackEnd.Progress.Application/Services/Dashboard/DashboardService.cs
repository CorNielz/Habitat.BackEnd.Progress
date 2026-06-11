using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Dashboard;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Services.Dashboard;

public sealed class DashboardService : IDashboardService
{
    private readonly IHabitRepository _habits;
    private readonly IHabitRecordRepository _records;
    private readonly INoteRepository _notes;
    private readonly IUserRepository _users;

    public DashboardService(IHabitRepository habits, IHabitRecordRepository records, INoteRepository notes, IUserRepository users)
    {
        _habits = habits;
        _records = records;
        _notes = notes;
        _users = users;
    }

    public async Task<Result<DashboardSummaryResponse>> GetSummaryAsync(int userId, DashboardPeriod period, CancellationToken cancellationToken = default)
    {
        if (!await ActiveUserExistsAsync(userId, cancellationToken))
        {
            return Result<DashboardSummaryResponse>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var (from, to) = DateRangeCalculator.Resolve(period);
        var habits = await _habits.ListActiveByUserAsync(userId, cancellationToken);
        var records = await _records.ListByUserBetweenAsync(userId, from, to, cancellationToken);
        var completedToday = await _records.CountByUserAndDateAsync(userId, today, cancellationToken);
        var notesCount = await _notes.CountByUserBetweenAsync(userId, from, to, cancellationToken);

        return Result<DashboardSummaryResponse>.Success(new DashboardSummaryResponse
        {
            Period = period,
            TotalHabits = habits.Count,
            CompletedToday = completedToday,
            CompletionRate = CalculateCompletionRate(habits, records, from, to),
            CurrentStreak = CalculateCurrentStreak(records),
            NotesCount = notesCount
        });
    }

    public async Task<Result<DashboardHistoryResponse>> GetHistoryAsync(int userId, DashboardPeriod period, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        if (!await ActiveUserExistsAsync(userId, cancellationToken))
        {
            return Result<DashboardHistoryResponse>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        var range = DateRangeCalculator.Resolve(period, from, to);
        var habits = await _habits.ListActiveByUserAsync(userId, cancellationToken);
        var records = await _records.ListByUserBetweenAsync(userId, range.From, range.To, cancellationToken);
        var activeHabitCount = habits.Count;

        var recordsByDate = records
            .GroupBy(r => r.RecordDate)
            .ToDictionary(g => g.Key, g => g.Select(r => r.HabitId).Distinct().Count());

        var items = new List<DashboardHistoryItem>();
        for (var date = range.From; date <= range.To; date = date.AddDays(1))
        {
            recordsByDate.TryGetValue(date, out var completed);
            items.Add(new DashboardHistoryItem
            {
                Date = date,
                CompletedHabits = completed,
                CompletionRate = activeHabitCount == 0 ? 0 : Math.Round(completed / (double)activeHabitCount * 100, 2)
            });
        }

        return Result<DashboardHistoryResponse>.Success(new DashboardHistoryResponse
        {
            Period = period,
            From = range.From,
            To = range.To,
            Items = items
        });
    }

    private async Task<bool> ActiveUserExistsAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is not null && user.IsActive;
    }

    private static double CalculateCompletionRate(IReadOnlyCollection<Habit> habits, IReadOnlyCollection<HabitRecord> records, DateOnly from, DateOnly to)
    {
        var expected = habits.Sum(h => HabitScheduleCalculator.CountExpectedOccurrences(h, from, to));
        if (expected <= 0)
        {
            return 0;
        }

        var completed = records.Select(r => new { r.HabitId, r.RecordDate }).Distinct().Count();
        return Math.Round(Math.Min(100, completed / (double)expected * 100), 2);
    }

    private static int CalculateCurrentStreak(IReadOnlyCollection<HabitRecord> records)
    {
        var completedDates = records.Select(r => r.RecordDate).Distinct().ToHashSet();
        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var streak = 0;
        for (var date = today; completedDates.Contains(date); date = date.AddDays(-1))
        {
            streak++;
        }

        return streak;
    }
}
