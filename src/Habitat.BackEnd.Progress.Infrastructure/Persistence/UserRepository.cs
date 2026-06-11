using Dapper;
using Habitat.BackEnd.Progress.Application.Common;
using Habitat.BackEnd.Progress.Application.Enums;
using Habitat.BackEnd.Progress.Application.Interfaces.Repositories;
using Habitat.BackEnd.Progress.Application.Models;
using Habitat.BackEnd.Progress.Infrastructure.Database;

namespace Habitat.BackEnd.Progress.Infrastructure.Persistence;

public sealed class UserRepository : IUserRepository
{
    private readonly IDbConnectionFactory _connectionFactory;
    private readonly IDatabaseRetryPolicy _retry;

    public UserRepository(IDbConnectionFactory connectionFactory, IDatabaseRetryPolicy retry)
    {
        _connectionFactory = connectionFactory;
        _retry = retry;
    }

    public Task<User?> GetByIdAsync(int id, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<User>(new CommandDefinition(UserSelectSql + " WHERE u.id = @Id", new { Id = id }, cancellationToken: ct));
    }, cancellationToken);

    public Task<User?> GetByEmailAsync(string email, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        return await connection.QuerySingleOrDefaultAsync<User>(new CommandDefinition(UserSelectSql + " WHERE u.email = @Email", new { Email = email }, cancellationToken: ct));
    }, cancellationToken);

    public Task<bool> EmailExistsAsync(string email, int? excludedUserId = null, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = """
            SELECT COUNT(1)
            FROM users
            WHERE email = @Email
              AND (@ExcludedUserId IS NULL OR id <> @ExcludedUserId);
            """;
        var count = await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { Email = email, ExcludedUserId = excludedUserId }, cancellationToken: ct));
        return count > 0;
    }, cancellationToken);

    public Task<User> CreateAsync(User user, UserSettings settings, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        try
        {
            var roleId = await GetRoleIdAsync(connection, transaction, user.Role, ct);
            user.RoleId = roleId;

            const string insertUserSql = """
                INSERT INTO users (role_id, name, email, password_hash, is_active, created_at, updated_at, last_login_at)
                VALUES (@RoleId, @Name, @Email, @PasswordHash, @IsActive, @CreatedAt, @UpdatedAt, @LastLoginAt);
                SELECT LAST_INSERT_ID();
                """;

            user.Id = await connection.ExecuteScalarAsync<int>(new CommandDefinition(insertUserSql, user, transaction, cancellationToken: ct));
            settings.UserId = user.Id;

            const string insertSettingsSql = """
                INSERT INTO user_settings (user_id, theme, default_dashboard_period, first_day_of_week, show_home_summary, updated_at)
                VALUES (@UserId, @Theme, @DefaultDashboardPeriod, @FirstDayOfWeek, @ShowHomeSummary, @UpdatedAt);
                """;

            await connection.ExecuteAsync(new CommandDefinition(insertSettingsSql, new
            {
                settings.UserId,
                Theme = settings.Theme.ToString(),
                DefaultDashboardPeriod = settings.DefaultDashboardPeriod.ToString(),
                FirstDayOfWeek = settings.FirstDayOfWeek.ToString(),
                settings.ShowHomeSummary,
                settings.UpdatedAt
            }, transaction, cancellationToken: ct));

            await transaction.CommitAsync(ct);
            return user;
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }, cancellationToken);

    public Task UpdateProfileAsync(int userId, string name, DateTime updatedAtUtc, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = "UPDATE users SET name = @Name, updated_at = @UpdatedAt WHERE id = @UserId;";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { UserId = userId, Name = name, UpdatedAt = updatedAtUtc }, cancellationToken: ct));
    }, cancellationToken);

    public Task UpdatePasswordAsync(int userId, string passwordHash, DateTime updatedAtUtc, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = "UPDATE users SET password_hash = @PasswordHash, updated_at = @UpdatedAt WHERE id = @UserId;";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { UserId = userId, PasswordHash = passwordHash, UpdatedAt = updatedAtUtc }, cancellationToken: ct));
    }, cancellationToken);

    public Task UpdateLastLoginAsync(int userId, DateTime lastLoginAtUtc, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string sql = "UPDATE users SET last_login_at = @LastLoginAt WHERE id = @UserId;";
        await connection.ExecuteAsync(new CommandDefinition(sql, new { UserId = userId, LastLoginAt = lastLoginAtUtc }, cancellationToken: ct));
    }, cancellationToken);

    public Task<PagedResponse<User>> ListAsync(PaginationRequest pagination, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        const string countSql = "SELECT COUNT(1) FROM users;";
        var total = await connection.ExecuteScalarAsync<int>(new CommandDefinition(countSql, cancellationToken: ct));
        var sql = UserSelectSql + " ORDER BY u.created_at DESC LIMIT @PageSize OFFSET @Offset";
        var items = (await connection.QueryAsync<User>(new CommandDefinition(sql, new { PageSize = pagination.SafePageSize, pagination.Offset }, cancellationToken: ct))).ToArray();
        return PagedResponse<User>.Create(items, total, pagination);
    }, cancellationToken);

    public Task UpdateRoleAsync(int userId, UserRole role, DateTime updatedAtUtc, CancellationToken cancellationToken = default) => _retry.ExecuteAsync(async ct =>
    {
        await using var connection = await _connectionFactory.CreateOpenConnectionAsync(ct);
        await using var transaction = await connection.BeginTransactionAsync(ct);
        try
        {
            var roleId = await GetRoleIdAsync(connection, transaction, role, ct);
            const string sql = "UPDATE users SET role_id = @RoleId, updated_at = @UpdatedAt WHERE id = @UserId;";
            await connection.ExecuteAsync(new CommandDefinition(sql, new { UserId = userId, RoleId = roleId, UpdatedAt = updatedAtUtc }, transaction, cancellationToken: ct));
            await transaction.CommitAsync(ct);
        }
        catch
        {
            await transaction.RollbackAsync(CancellationToken.None);
            throw;
        }
    }, cancellationToken);

    private static async Task<int> GetRoleIdAsync(System.Data.IDbConnection connection, System.Data.IDbTransaction transaction, UserRole role, CancellationToken ct)
    {
        const string sql = "SELECT id FROM roles WHERE name = @Role;";
        return await connection.ExecuteScalarAsync<int>(new CommandDefinition(sql, new { Role = role.ToString() }, transaction, cancellationToken: ct));
    }

    private const string UserSelectSql = """
        SELECT
            u.id AS Id,
            u.role_id AS RoleId,
            CASE r.name WHEN 'ADMIN' THEN 1 ELSE 0 END AS Role,
            u.name AS Name,
            u.email AS Email,
            u.password_hash AS PasswordHash,
            u.is_active AS IsActive,
            u.created_at AS CreatedAt,
            u.updated_at AS UpdatedAt,
            u.last_login_at AS LastLoginAt
        FROM users u
        INNER JOIN roles r ON r.id = u.role_id
        """;
}
