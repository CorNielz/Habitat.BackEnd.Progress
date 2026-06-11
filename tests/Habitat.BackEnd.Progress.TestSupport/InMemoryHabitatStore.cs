using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;

namespace Habitat.BackEnd.Progress.TestSupport;

public sealed class InMemoryHabitatStore : IUserRepository, IUserSettingsRepository, IHabitRepository, IHabitRecordRepository, INoteRepository
{
    private int _nextUserId = 1;
    private int _nextSettingsId = 1;
    private int _nextHabitId = 1;
    private int _nextRecordId = 1;
    private int _nextNoteId = 1;

    public List<User> Users { get; } = new();
    public List<UserSettings> Settings { get; } = new();
    public List<Habit> Habits { get; } = new();
    public List<HabitRecord> Records { get; } = new();
    public List<Note> Notes { get; } = new();

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.FirstOrDefault(u => u.Id == id));

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.FirstOrDefault(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase)));

    public Task<bool> EmailExistsAsync(string email, int? excludedUserId = null, CancellationToken cancellationToken = default) =>
        Task.FromResult(Users.Any(u => string.Equals(u.Email, email, StringComparison.OrdinalIgnoreCase) && (!excludedUserId.HasValue || u.Id != excludedUserId.Value)));

    public Task<User> CreateAsync(User user, UserSettings settings, CancellationToken cancellationToken = default)
    {
        user.Id = _nextUserId++;
        user.RoleId = user.Role == UserRole.ADMIN ? 2 : 1;
        Users.Add(Clone(user));
        settings.Id = _nextSettingsId++;
        settings.UserId = user.Id;
        Settings.Add(Clone(settings));
        return Task.FromResult(Clone(user));
    }

    public Task UpdateProfileAsync(int userId, string name, DateTime updatedAtUtc, CancellationToken cancellationToken = default)
    {
        var user = Users.First(u => u.Id == userId);
        user.Name = name;
        user.UpdatedAt = updatedAtUtc;
        return Task.CompletedTask;
    }

    public Task UpdatePasswordAsync(int userId, string passwordHash, DateTime updatedAtUtc, CancellationToken cancellationToken = default)
    {
        var user = Users.First(u => u.Id == userId);
        user.PasswordHash = passwordHash;
        user.UpdatedAt = updatedAtUtc;
        return Task.CompletedTask;
    }

    public Task UpdateLastLoginAsync(int userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken = default)
    {
        var user = Users.First(u => u.Id == userId);
        user.LastLoginAt = lastLoginAtUtc;
        return Task.CompletedTask;
    }

    public Task<PagedResponse<User>> ListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var items = Users.OrderByDescending(u => u.CreatedAt).Skip(pagination.Offset).Take(pagination.SafePageSize).Select(Clone).ToArray();
        return Task.FromResult(PagedResponse<User>.Create(items, Users.Count, pagination));
    }

    public Task UpdateRoleAsync(int userId, UserRole role, DateTime updatedAtUtc, CancellationToken cancellationToken = default)
    {
        var user = Users.First(u => u.Id == userId);
        user.Role = role;
        user.RoleId = role == UserRole.ADMIN ? 2 : 1;
        user.UpdatedAt = updatedAtUtc;
        return Task.CompletedTask;
    }

    public Task<UserSettings?> GetByUserIdAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Settings.FirstOrDefault(s => s.UserId == userId) is { } settings ? Clone(settings) : null);

    public Task<UserSettings> UpsertAsync(UserSettings settings, CancellationToken cancellationToken = default)
    {
        var existing = Settings.FirstOrDefault(s => s.UserId == settings.UserId);
        if (existing is null)
        {
            settings.Id = _nextSettingsId++;
            Settings.Add(Clone(settings));
            return Task.FromResult(Clone(settings));
        }

        existing.Theme = settings.Theme;
        existing.DefaultDashboardPeriod = settings.DefaultDashboardPeriod;
        existing.FirstDayOfWeek = settings.FirstDayOfWeek;
        existing.ShowHomeSummary = settings.ShowHomeSummary;
        existing.UpdatedAt = settings.UpdatedAt;
        return Task.FromResult(Clone(existing));
    }

    public Task<PagedResponse<Habit>> ListByUserAsync(int userId, PaginationRequest pagination, CancellationToken cancellationToken = default)
    {
        var query = Habits.Where(h => h.UserId == userId).OrderByDescending(h => h.CreatedAt).ToArray();
        var items = query.Skip(pagination.Offset).Take(pagination.SafePageSize).Select(Clone).ToArray();
        return Task.FromResult(PagedResponse<Habit>.Create(items, query.Length, pagination));
    }

    public Task<IReadOnlyCollection<Habit>> ListActiveByUserAsync(int userId, CancellationToken cancellationToken = default) =>
        Task.FromResult((IReadOnlyCollection<Habit>)Habits.Where(h => h.UserId == userId && h.IsActive).Select(Clone).ToArray());

    Task<Habit?> IHabitRepository.GetByIdForUserAsync(int userId, int habitId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Habits.FirstOrDefault(h => h.UserId == userId && h.Id == habitId) is { } habit ? Clone(habit) : null);

    public Task<Habit> CreateAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        habit.Id = _nextHabitId++;
        Habits.Add(Clone(habit));
        return Task.FromResult(Clone(habit));
    }

    public Task<bool> UpdateAsync(Habit habit, CancellationToken cancellationToken = default)
    {
        var index = Habits.FindIndex(h => h.Id == habit.Id && h.UserId == habit.UserId);
        if (index < 0) return Task.FromResult(false);
        Habits[index] = Clone(habit);
        return Task.FromResult(true);
    }

    Task<bool> IHabitRepository.DeleteAsync(int userId, int habitId, CancellationToken cancellationToken)
    {
        var removed = Habits.RemoveAll(h => h.Id == habitId && h.UserId == userId) > 0;

        if (removed)
        {
            Records.RemoveAll(r => r.HabitId == habitId);
        }

        return Task.FromResult(removed);
    }

    public Task<PagedResponse<HabitRecord>> ListByHabitAsync(int habitId, PaginationRequest pagination, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default)
    {
        var query = Records.Where(r => r.HabitId == habitId && (!from.HasValue || r.RecordDate >= from.Value) && (!to.HasValue || r.RecordDate <= to.Value))
            .OrderByDescending(r => r.RecordDate)
            .ToArray();
        var items = query.Skip(pagination.Offset).Take(pagination.SafePageSize).Select(Clone).ToArray();
        return Task.FromResult(PagedResponse<HabitRecord>.Create(items, query.Length, pagination));
    }

    public Task<IReadOnlyCollection<HabitRecord>> ListByUserBetweenAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default)
    {
        var habitIds = Habits.Where(h => h.UserId == userId && h.IsActive).Select(h => h.Id).ToHashSet();
        var records = Records.Where(r => habitIds.Contains(r.HabitId) && r.RecordDate >= from && r.RecordDate <= to).Select(Clone).ToArray();
        return Task.FromResult((IReadOnlyCollection<HabitRecord>)records);
    }

    public Task<int> CountByUserAndDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default)
    {
        var habitIds = Habits.Where(h => h.UserId == userId && h.IsActive).Select(h => h.Id).ToHashSet();
        return Task.FromResult(Records.Where(r => habitIds.Contains(r.HabitId) && r.RecordDate == date).Select(r => r.HabitId).Distinct().Count());
    }

    public Task<bool> ExistsAsync(int habitId, DateOnly recordDate, CancellationToken cancellationToken = default) =>
        Task.FromResult(Records.Any(r => r.HabitId == habitId && r.RecordDate == recordDate));

    public Task<HabitRecord> CreateAsync(HabitRecord record, CancellationToken cancellationToken = default)
    {
        record.Id = _nextRecordId++;
        Records.Add(Clone(record));
        return Task.FromResult(Clone(record));
    }

    public Task<bool> DeleteByDateAsync(int habitId, DateOnly recordDate, CancellationToken cancellationToken = default) =>
        Task.FromResult(Records.RemoveAll(r => r.HabitId == habitId && r.RecordDate == recordDate) > 0);

    public Task<PagedResponse<Note>> ListByUserAsync(int userId, PaginationRequest pagination, DateOnly? date, CancellationToken cancellationToken = default)
    {
        var query = Notes.Where(n => n.UserId == userId && (!date.HasValue || n.Date == date.Value)).OrderByDescending(n => n.Date).ToArray();
        var items = query.Skip(pagination.Offset).Take(pagination.SafePageSize).Select(Clone).ToArray();
        return Task.FromResult(PagedResponse<Note>.Create(items, query.Length, pagination));
    }

    public Task<int> CountByUserBetweenAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) =>
        Task.FromResult(Notes.Count(n => n.UserId == userId && n.Date >= from && n.Date <= to));

    Task<Note?> INoteRepository.GetByIdForUserAsync(int userId, int noteId, CancellationToken cancellationToken = default) =>
        Task.FromResult(Notes.FirstOrDefault(n => n.UserId == userId && n.Id == noteId) is { } note ? Clone(note) : null);

    public Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default)
    {
        note.Id = _nextNoteId++;
        Notes.Add(Clone(note));
        return Task.FromResult(Clone(note));
    }

    public Task<bool> UpdateAsync(Note note, CancellationToken cancellationToken = default)
    {
        var index = Notes.FindIndex(n => n.Id == note.Id && n.UserId == note.UserId);
        if (index < 0) return Task.FromResult(false);
        Notes[index] = Clone(note);
        return Task.FromResult(true);
    }

    Task<bool> INoteRepository.DeleteAsync(int userId, int noteId, CancellationToken cancellationToken) =>
        Task.FromResult(Notes.RemoveAll(n => n.Id == noteId && n.UserId == userId) > 0);

    private static User Clone(User user) => new() { Id = user.Id, RoleId = user.RoleId, Role = user.Role, Name = user.Name, Email = user.Email, PasswordHash = user.PasswordHash, IsActive = user.IsActive, CreatedAt = user.CreatedAt, UpdatedAt = user.UpdatedAt, LastLoginAt = user.LastLoginAt };
    private static UserSettings Clone(UserSettings settings) => new() { Id = settings.Id, UserId = settings.UserId, Theme = settings.Theme, DefaultDashboardPeriod = settings.DefaultDashboardPeriod, FirstDayOfWeek = settings.FirstDayOfWeek, ShowHomeSummary = settings.ShowHomeSummary, UpdatedAt = settings.UpdatedAt };
    private static Habit Clone(Habit habit) => new() { Id = habit.Id, UserId = habit.UserId, Title = habit.Title, Description = habit.Description, FrequencyType = habit.FrequencyType, FrequencyValue = habit.FrequencyValue, StartDate = habit.StartDate, IsActive = habit.IsActive, CreatedAt = habit.CreatedAt, UpdatedAt = habit.UpdatedAt };
    private static HabitRecord Clone(HabitRecord record) => new() { Id = record.Id, HabitId = record.HabitId, RecordDate = record.RecordDate, Completed = record.Completed, Note = record.Note, RecordedAt = record.RecordedAt };
    private static Note Clone(Note note) => new() { Id = note.Id, UserId = note.UserId, Title = note.Title, Content = note.Content, Date = note.Date, CreatedAt = note.CreatedAt, UpdatedAt = note.UpdatedAt };
}
