namespace FluentRetry;

[ExcludeFromCodeCoverage]
public static class Retry
{
    public static GenericRetryAsync<T> WithResultAsync<T>(Func<Task<T>> runner)
    {
        return new GenericRetryAsync<T>(runner);
    }

    public static GenericRetry<T> WithResult<T>(Func<T> runner)
    {
        return new GenericRetry<T>(runner);
    }

    public static SimpleRetryAsync WithAsync(Func<Task> runner)
    {
        return new SimpleRetryAsync(runner);
    }

    public static SimpleRetry With(Action runner)
    {
        return new SimpleRetry(runner);
    }

    public static void SetGlobalRetryConfiguration(RetryConfiguration retryConfiguration)
    {
        RetryInternals.RetryConfiguration
            = retryConfiguration ?? throw new ArgumentNullException(nameof(retryConfiguration));
    }

    public static void SetGlobalLogHandler(Action<RetryLog> logHandler)
    {
        RetryInternals.RetryConfiguration.LogHandler
            = logHandler ?? throw new ArgumentNullException(nameof(logHandler));
    }
}
