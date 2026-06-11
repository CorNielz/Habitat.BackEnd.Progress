using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MySqlConnector;

namespace Habitat.BackEnd.Progress.Infrastructure.Database;

public sealed class MySqlRetryPolicy : IDatabaseRetryPolicy
{
    private static readonly HashSet<int> TransientErrorNumbers = new()
    {
        0,
        1042, // unable to connect to any of the specified MySQL hosts
        1205, // lock wait timeout
        1213, // deadlock
        2006, // server has gone away
        2013  // lost connection during query
    };

    private readonly DatabaseRetryOptions _options;
    private readonly ILogger<MySqlRetryPolicy> _logger;

    public MySqlRetryPolicy(IOptions<DatabaseRetryOptions> options, ILogger<MySqlRetryPolicy> logger)
    {
        _options = options.Value;
        _logger = logger;
    }

    public Task ExecuteAsync(Func<CancellationToken, Task> operation, CancellationToken cancellationToken = default) =>
        ExecuteAsync(async ct =>
        {
            await operation(ct);
            return true;
        }, cancellationToken);

    public async Task<T> ExecuteAsync<T>(Func<CancellationToken, Task<T>> operation, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(operation);

        var attempts = Math.Max(1, _options.MaxAttempts);
        Exception? lastException = null;

        for (var attempt = 1; attempt <= attempts; attempt++)
        {
            try
            {
                return await operation(cancellationToken);
            }
            catch (Exception ex) when (IsTransient(ex) && attempt < attempts)
            {
                lastException = ex;
                var delay = ComputeDelay(attempt);
                _logger.LogWarning(ex, "Transient database error. Retrying attempt {Attempt}/{Attempts} in {Delay}ms.", attempt + 1, attempts, delay.TotalMilliseconds);
                await Task.Delay(delay, cancellationToken);
            }
        }

        throw lastException ?? new InvalidOperationException("Database operation failed without an exception.");
    }

    private TimeSpan ComputeDelay(int attempt)
    {
        var baseDelay = Math.Max(50, _options.BaseDelayMilliseconds);
        var jitter = Random.Shared.Next(0, 75);
        return TimeSpan.FromMilliseconds(baseDelay * Math.Pow(2, attempt - 1) + jitter);
    }

    private static bool IsTransient(Exception exception) => exception switch
    {
        MySqlException mysqlException => TransientErrorNumbers.Contains(mysqlException.Number),
        TimeoutException => true,
        _ when exception.InnerException is not null => IsTransient(exception.InnerException),
        _ => false
    };
}
