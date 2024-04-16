namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryConfiguration
{
    public int RetryCount { get; init; } = 3;
    public int RetrySleepInMs { get; init; } = 150;
}
