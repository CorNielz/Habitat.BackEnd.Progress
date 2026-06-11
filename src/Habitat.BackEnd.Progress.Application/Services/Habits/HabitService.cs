using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.DTOs.Habits;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Interfaces.Services;
using Habitat.BackEnd.Progress.Application.Mappings;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.Application.Services.Habits;

public sealed class HabitService : IHabitService
{
    private readonly IHabitRepository _habits;
    private readonly IUserRepository _users;

    public HabitService(IHabitRepository habits, IUserRepository users)
    {
        _habits = habits;
        _users = users;
    }

    public async Task<Result<PagedResponse<HabitResponse>>> ListAsync(int userId, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        if (!await ActiveUserExistsAsync(userId, cancellationToken))
        {
            return Result<PagedResponse<HabitResponse>>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        var habits = await _habits.ListByUserAsync(userId, pagination, cancellationToken);
        return Result<PagedResponse<HabitResponse>>.Success(PagedResponse<HabitResponse>.Create(
            habits.Items.Select(h => h.ToResponse()).ToArray(),
            habits.TotalItems,
            pagination));
    }

    public async Task<Result<HabitResponse>> GetByIdAsync(int userId, int habitId, CancellationToken cancellationToken = default)
    {
        var habit = await _habits.GetByIdForUserAsync(userId, habitId, cancellationToken);
        return habit is null
            ? Result<HabitResponse>.NotFound("habits.not_found", "The requested habit was not found.")
            : Result<HabitResponse>.Success(habit.ToResponse());
    }

    public async Task<Result<HabitResponse>> CreateAsync(int userId, CreateHabitRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        if (!await ActiveUserExistsAsync(userId, cancellationToken))
        {
            return Result<HabitResponse>.NotFound("users.not_found", "The authenticated user was not found.");
        }

        var validation = ValidateHabit(request.Title, request.FrequencyValue, request.StartDate);
        if (validation is not null)
        {
            return Result<HabitResponse>.Validation(validation.Code, validation.Message);
        }

        var now = DateTime.UtcNow;
        var habit = new Habit
        {
            UserId = userId,
            Title = request.Title.Trim(),
            Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim(),
            FrequencyType = request.FrequencyType,
            FrequencyValue = request.FrequencyValue.Trim(),
            StartDate = request.StartDate,
            IsActive = true,
            CreatedAt = now,
            UpdatedAt = now
        };

        var created = await _habits.CreateAsync(habit, cancellationToken);
        return Result<HabitResponse>.Success(created.ToResponse());
    }

    public async Task<Result<HabitResponse>> UpdateAsync(int userId, int habitId, UpdateHabitRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);

        var habit = await _habits.GetByIdForUserAsync(userId, habitId, cancellationToken);
        if (habit is null)
        {
            return Result<HabitResponse>.NotFound("habits.not_found", "The requested habit was not found.");
        }

        var validation = ValidateHabit(request.Title, request.FrequencyValue, request.StartDate);
        if (validation is not null)
        {
            return Result<HabitResponse>.Validation(validation.Code, validation.Message);
        }

        habit.Title = request.Title.Trim();
        habit.Description = string.IsNullOrWhiteSpace(request.Description) ? null : request.Description.Trim();
        habit.FrequencyType = request.FrequencyType;
        habit.FrequencyValue = request.FrequencyValue.Trim();
        habit.StartDate = request.StartDate;
        habit.UpdatedAt = DateTime.UtcNow;

        var updated = await _habits.UpdateAsync(habit, cancellationToken);
        return updated
            ? Result<HabitResponse>.Success(habit.ToResponse())
            : Result<HabitResponse>.NotFound("habits.not_found", "The requested habit was not found.");
    }

    public async Task<Result> DeleteAsync(int userId, int habitId, CancellationToken cancellationToken = default)
    {
        var deleted = await _habits.DeleteAsync(userId, habitId, cancellationToken);
        return deleted
            ? Result.Success()
            : Result.NotFound("habits.not_found", "The requested habit was not found.");
    }

    private async Task<bool> ActiveUserExistsAsync(int userId, CancellationToken cancellationToken)
    {
        var user = await _users.GetByIdAsync(userId, cancellationToken);
        return user is not null && user.IsActive;
    }

    private static Error? ValidateHabit(string title, string frequencyValue, DateOnly startDate)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            return new Error("habits.invalid_title", "Habit title is required.");
        }

        if (string.IsNullOrWhiteSpace(frequencyValue))
        {
            return new Error("habits.invalid_frequency", "Habit frequency value is required.");
        }

        if (startDate == default)
        {
            return new Error("habits.invalid_start_date", "Habit start date is required.");
        }

        return null;
    }
}
