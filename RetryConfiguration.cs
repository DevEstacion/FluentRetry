using System;

namespace FluentRetry;

public class RetryConfiguration
{
    public Action<RetryLog> LogHandler { get; init; } = delegate { };
    public int RetryCount { get; init; }
    public int RetrySleepInSeconds { get; init; }
}
