using Dapper;
using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Infrastructure.Database;

namespace Habitat.BackEnd.Progress.Infrastructure.Persistence;

public sealed class NoteRepository : INoteRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDatabaseRetryPolicy _retry;

    public NoteRepository(IDbConnectionFactory connectionFactory, IDatabaseRetryPolicy retry)
    {
        _connectionFactory = connectionFactory;
        _retry = retry;
    }

    public Task<PagedResponse<Note>> ListByUserAsync(int userId, PaginationRequest pagination, DateOnly? date, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string countSql = """
            SELECT COUNT(1)
            FROM notes
            WHERE user_id = @UserId
              AND (@Date IS NULL OR note_date = @Date);
            """;
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, new { UserId = userId, Date = date }, cancellationToken: ct));
        var sql = NoteSelectSql + " WHERE user_id = @UserId AND (@Date IS NULL OR note_date = @Date) ORDER BY note_date DESC, created_at DESC LIMIT @PageSize OFFSET @Offset;";
        var items = (await connection.QueryAsync<Note>(new CommandDefinition(sql, new { UserId = userId, Date = date, PageSize = pagination.SafePageSize, pagination.Offset }, cancellationToken: ct))).ToArray();
        return PagedResponse<Note>.Create(items, total, pagination);
    }, cancellationToken);

    public Task<int> CountByUserBetweenAsync(int userId, DateOnly from, DateOnly to, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = "SELECT COUNT(1) FROM notes WHERE user_id = @UserId AND note_date BETWEEN @From AND @To;";
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { UserId = userId, From = from, To = to }, cancellationToken: ct));
    }, cancellationToken);

    public Task<Note?> GetByIdForUserAsync(int userId, int noteId, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        var sql = NoteSelectSql + " WHERE user_id = @UserId AND id = @NoteId;";
        return await connection.QuerySingleOrDefaultAsync<Note>(new CommandDefinition(sql, new { UserId = userId, NoteId = noteId }, cancellationToken: ct));
    }, cancellationToken);

    public Task<Note> CreateAsync(Note note, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            INSERT INTO notes (user_id, title, content, note_date, created_at, updated_at)
            VALUES (@UserId, @Title, @Content, @Date, @CreatedAt, @UpdatedAt);
            SELECT LAST_INSERT_ID();
            """;
        note.Id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, note, cancellationToken: ct));
        return note;
    }, cancellationToken);

    public Task<bool> UpdateAsync(Note note, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            UPDATE notes
            SET title = @Title, content = @Content, note_date = @Date, updated_at = @UpdatedAt
            WHERE id = @Id AND user_id = @UserId;
            """;
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, note, cancellationToken: ct));
        return affected > 0;
    }, cancellationToken);

    public Task<bool> DeleteAsync(int userId, int noteId, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = "DELETE FROM notes WHERE id = @NoteId AND user_id = @UserId;";
        var affected = await connection.ExecuteAsync(new CommandDefinition(sql, new { UserId = userId, NoteId = noteId }, cancellationToken: ct));
        return affected > 0;
    }, cancellationToken);

    private const string NoteSelectSql = """
        SELECT id AS Id, user_id AS UserId, title AS Title, content AS Content,
               note_date AS Date, created_at AS CreatedAt, updated_at AS UpdatedAt
        FROM notes
        """;
}
