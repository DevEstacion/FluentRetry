namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryConfiguration
{
    public Action<RetryLog> LogHandler { get; init; } = delegate { };
    public int RetryCount { get; init; }
    public int RetrySleepInMs { get; init; }
}
