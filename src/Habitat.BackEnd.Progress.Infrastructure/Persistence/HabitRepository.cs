using Dapper;
using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Infrastructure.Database;

namespace Habitat.BackEnd.Progress.Infrastructure.Persistence;

public sealed class HabitRepository : IHabitRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDatabaseRetryPolicy _retry;

    public HabitRepository(IDbConnectionFactory connectionFactory, IDatabaseRetryPolicy retry)
    {
        _connectionFactory = connectionFactory;
        _retry = retry;
    }

    public Task<PagedResponse<Habit>> ListByUserAsync(int userId, PaginationRequest pagination, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string countSql = "SELECT COUNT(1) FROM habits WHERE user_id = @UserId;";
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, new { UserId = userId }, cancellationToken: ct));
        var sql = HabitSelectSql + " WHERE user_id = @UserId ORDER BY created_at DESC LIMIT @PageSize OFFSET @Offset;";
        var rows = await connection.QueryAsync<HabitRow>(new CommandDefinition(sql, new { UserId = userId, PageSize = pagination.SafePageSize, pagination.Offset }, cancellationToken: ct));
        return PagedResponse<Habit>.Create(rows.Select(r => r.ToHabit()).ToArray(), total, pagination);
    }, cancellationToken);

    public Task<IReadOnlyCollection<Habit>> ListActiveByUserAsync(int userId, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        var sql = HabitSelectSql + " WHERE user_id = @UserId AND is_active = TRUE ORDER BY created_at;";
        var rows = await connection.QueryAsync<HabitRow>(new CommandDefinition(sql, new { UserId = userId }, cancellationToken: ct));
        return (IReadOnlyCollection<Habit>)rows.Select(r => r.ToHabit()).ToArray();
    }, cancellationToken);

    public Task<Habit?> GetByIdForUserAsync(int userId, int habitId, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        var sql = HabitSelectSql + " WHERE user_id = @UserId AND id = @HabitId;";
        var row = await connection.QuerySingleOrDefaultAsync<HabitRow>(new CommandDefinition(sql, new { UserId = userId, HabitId = habitId }, cancellationToken: ct));
        return row?.ToHabit();
    }, cancellationToken);

    public Task<Habit> CreateAsync(Habit habit, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            INSERT INTO habits (user_id, title, description, frequency_type, frequency_value, start_date, is_active, created_at, updated_at)
            VALUES (@UserId, @Title, @Description, @FrequencyType, @FrequencyValue, @StartDate, @IsActive, @CreatedAt, @UpdatedAt);
            SELECT LAST_INSERT_ID();
            """;
        habit.Id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new
        {
            habit.UserId,
            habit.Title,
            habit.Description,
            FrequencyType = habit.FrequencyType.ToString(),
            habit.FrequencyValue,
            habit.StartDate,
            habit.IsActive,
            habit.CreatedAt,
            habit.UpdatedAt
        }, cancellationToken: ct));
        return habit;
    }, cancellationToken);

    public Task<bool> UpdateAsync(Habit habit, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            UPDATE habits
            SET title = @Title,
                description = @Description,
                frequency_type = @FrequencyType,
                frequency_value = @FrequencyValue,
                start_date = @StartDate,
                updated_at = @UpdatedAt
            WHERE id = @Id AND user_id = @UserId;
            """;
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new
        {
            habit.Id,
            habit.UserId,
            habit.Title,
            habit.Description,
            FrequencyType = habit.FrequencyType.ToString(),
            habit.FrequencyValue,
            habit.StartDate,
            habit.UpdatedAt
        }, cancellationToken: ct));
        return affected > 0;
    }, cancellationToken);

    public Task<bool> DeleteAsync(int userId, int habitId, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = "DELETE FROM habits WHERE id = @HabitId AND user_id = @UserId;";
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { UserId = userId, HabitId = habitId }, cancellationToken: ct));
        return affected > 0;
    }, cancellationToken);

    private const string HabitSelectSql = """
        SELECT id AS Id, user_id AS UserId, title AS Title, description AS Description,
               frequency_type AS FrequencyType, frequency_value AS FrequencyValue,
               start_date AS StartDate, is_active AS IsActive, created_at AS CreatedAt, updated_at AS UpdatedAt
        FROM habits
        """;

    private sealed class HabitRow
    {
        public int Id { get; init; }
        public int UserId { get; init; }
        public string Title { get; init; } = string.Empty;
        public string? Description { get; init; }
        public string FrequencyType { get; init; } = string.Empty;
        public string FrequencyValue { get; init; } = string.Empty;
        public DateOnly StartDate { get; init; }
        public bool IsActive { get; init; }
        public DateTime CreatedAt { get; init; }
        public DateTime UpdatedAt { get; init; }

        public Habit ToHabit() => new()
        {
            Id = Id,
            UserId = UserId,
            Title = Title,
            Description = Description,
            FrequencyType = Enum.Parse<HabitFrequencyType>(FrequencyType, ignoreCase: true),
            FrequencyValue = FrequencyValue,
            StartDate = StartDate,
            IsActive = IsActive,
            CreatedAt = CreatedAt,
            UpdatedAt = UpdatedAt
        };
    }
}
