namespace FluentRetry;

[ExcludeFromCodeCoverage]
internal static class Internals
{
    private static RetryConfiguration _retryConfiguration;

    public static RetryConfiguration RetryConfiguration => _retryConfiguration ??= new RetryConfiguration
    {
        RetryCount = 3,
        RetrySleepInMs = 150,
        LogHandler = delegate { }
    };
}
