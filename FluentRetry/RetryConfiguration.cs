namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryConfiguration
{

    public int RetryCount { get; init; } = 3;
    public int RetrySleepInMs { get; init; } = 150;

    /// <summary>
    ///     Sets the range of the jitter added to each retry
    /// </summary>
    public Jitter Jitter { get; init; } = Jitter.Range(10, 100);
}
