namespace FluentRetry;

[ExcludeFromCodeCoverage]
public static class Retry
{
    internal static RetryConfiguration RetryConfiguration { get; set; } = new();

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
        RetryConfiguration
            = retryConfiguration ?? throw new ArgumentNullException(nameof(retryConfiguration));
    }
}
