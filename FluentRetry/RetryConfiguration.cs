namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryConfiguration
{
    public Action<RetryLog> LogHandler { get; internal set; } = delegate { };
    public int RetryCount { get; init; } = 3;
    public int RetrySleepInMs { get; init; } = 150;
}
