using Dapper;
using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Infrastructure.Database;

namespace Habitat.BackEnd.Progress.Infrastructure.Persistence;

public sealed class HabitRecordRepository : IHabitRecordRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDatabaseRetryPolicy _retry;

    public HabitRecordRepository(IDbConnectionFactory connectionFactory, IDatabaseRetryPolicy retry)
    {
        _connectionFactory = connectionFactory;
        _retry = retry;
    }

    public Task<PagedResponse<HabitRecord>> ListByHabitAsync(int habitId, PaginationRequest pagination, DateOnly? from, DateOnly? to, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string countSql = """
            SELECT COUNT(1)
            FROM habit_records
            WHERE habit_id = @HabitId
              AND (@From IS NULL OR record_date >= @From)
              AND (@To IS NULL OR record_date <= @To);
            """;
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, new { HabitId = habitId, From = from, To = to }, cancellationToken: ct));
        const string sql = """
            SELECT id AS Id, habit_id AS HabitId, record_date AS RecordDate, completed AS Completed, note AS Note, recorded_at AS RecordedAt
            FROM habit_records
            WHERE habit_id = @HabitId
              AND (@From IS NULL OR record_date >= @From)
              AND (@To IS NULL OR record_date <= @To)
            ORDER BY record_date DESC
            LIMIT @PageSize OFFSET @Offset;
            """;
        var items = (await connection.QueryAsync<HabitRecord>(new CommandDefinition(sql, new { HabitId = habitId, From = from, To = to, PageSize = pagination.SafePageSize, pagination.Offset }, cancellationToken: ct))).ToArray();
        return PagedResponse<HabitRecord>.Create(items, total, pagination);
    }, cancellationToken);

    public Task<IReadOnlyCollection<HabitRecord>> ListByUserBetweenAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            SELECT hr.id AS Id, hr.habit_id AS HabitId, hr.record_date AS RecordDate, hr.completed AS Completed, hr.note AS Note, hr.recorded_at AS RecordedAt
            FROM habit_records hr
            INNER JOIN habits h ON h.id = hr.habit_id
            WHERE h.user_id = @UserId
              AND h.is_active = TRUE
              AND hr.completed = TRUE
              AND hr.record_date BETWEEN @From AND @To;
            """;
        var records = await connection.QueryAsync<HabitRecord>(new CommandDefinition(sql, new { UserId = userId, From = from, To = to }, cancellationToken: ct));
        return (IReadOnlyCollection<HabitRecord>)records.ToArray();
    }, cancellationToken);

    public Task<int> CountByUserAndDateAsync(int userId, DateOnly date, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            SELECT COUNT(DISTINCT hr.habit_id)
            FROM habit_records hr
            INNER JOIN habits h ON h.id = hr.habit_id
            WHERE h.user_id = @UserId AND h.is_active = TRUE AND hr.completed = TRUE AND hr.record_date = @Date;
            """;
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { UserId = userId, Date = date }, cancellationToken: ct));
    }, cancellationToken);

    public Task<bool> ExistsAsync(int habitId, DateOnly recordDate, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = "SELECT COUNT(1) FROM habit_records WHERE habit_id = @HabitId AND record_date = @RecordDate;";
        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { HabitId = habitId, RecordDate = recordDate }, cancellationToken: ct));
        return count > 0;
    }, cancellationToken);

    public Task<HabitRecord> CreateAsync(HabitRecord record, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            INSERT INTO habit_records (habit_id, record_date, completed, note, recorded_at)
            VALUES (@HabitId, @RecordDate, @Completed, @Note, @RecordedAt);
            SELECT LAST_INSERT_ID();
            """;
        record.Id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, record, cancellationToken: ct));
        return record;
    }, cancellationToken);

    public Task<bool> DeleteByDateAsync(int habitId, DateOnly recordDate, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = "DELETE FROM habit_records WHERE habit_id = @HabitId AND record_date = @RecordDate;";
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { HabitId = habitId, RecordDate = recordDate }, cancellationToken: ct));
        return affected > 0;
    }, cancellationToken);
}
