using Microsoft.Extensions.Configuration;
using MySqlConnector;

namespace Habitat.BackEnd.Progress.Infrastructure.Database;

public sealed class MySqlConnectionFactory : IDbConnectionFactory
{
    private readonly string _connectionString;

    public MySqlConnectionFactory(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString("HabitatDatabase")
            ?? throw new InvalidOperationException("Connection string 'HabitatDatabase' is required.");
    }

    public async Task<MySqlConnection> CreateOpenConnectionAsync(CancellationToken cancellationToken = default)
    {
        var connection = new MySqlConnection(_connectionString);
        await connection.OpenAsync(cancellationToken);
        return connection;
    }
}
