using MySqlConnector;

namespace Habitat.BackEnd.Progress.Infrastructure.Database;

public interface IDbConnectionFactory
{
    Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default);
}
