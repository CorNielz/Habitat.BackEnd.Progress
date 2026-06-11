namespace Habitat.BackEnd.Progress.Infrastructure.Database;

public sealed class DatabaseRetryOptions
{
    public const string SectionName = "DatabaseRetry";

    public int MaxAttempts { get; init; } = 3;
    public int BaseDelayMilliseconds { get; init; } = 150;
}
