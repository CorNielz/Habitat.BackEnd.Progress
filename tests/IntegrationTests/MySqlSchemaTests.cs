using Dapper;
using MySqlConnector;

namespace IntegrationTests;

public sealed class MySqlSchemaTests
{
    [Fact]
    public async Task Database_HasExpectedCoreTables_WhenIntegrationTestsAreEnabled()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("RUN_INTEGRATION_TESTS"), "true", StringComparison.OrdinalIgnoreCase))
        {
            return;
        }

        var connectionString = Environment.GetEnvironmentVariable("ConnectionStrings__HabitatDatabase")
            ?? "Server=localhost;Port=3306;Database=habitat_progress;User=habitat_user;Password=habitat_password;SslMode=Preferred;AllowPublicKeyRetrieval=True;";

        await using var connection = new MySqlConnection(connectionString);
        await connection.OpenAsync();

        string[] tables = ["roles", "users", "user_settings", "habits", "habit_records", "notes"];
        foreach (var table in tables)
        {
            var count = await connection.ExecuteScalarAsync<int>("SELECT COUNT(1) FROM information_schema.tables WHERE table_schema = DATABASE() AND table_name = @Table;", new { Table = table });
            Assert.Equal(1, count);
        }
    }
}
