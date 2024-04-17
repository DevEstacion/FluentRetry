namespace FluentRetry;

[ExcludeFromCodeCoverage]
public static class Retry
{
    internal static RetryConfiguration RetryConfiguration { get; set; } = new();

    /// <summary>
    ///     Returns an instance of a fluent retry class that can handle <see cref="Task" /> with results
    /// </summary>
    /// <typeparam name="T">Type of results the task returns</typeparam>
    public static GenericRetryAsync<T> WithResultAsync<T>(Func<Task<T>> runner)
    {
        return new GenericRetryAsync<T>(runner);
    }

    /// <summary>
    ///     Returns an instance of a fluent retry class that can handle results
    /// </summary>
    /// <typeparam name="T">Type of results the delegate returns</typeparam>
    public static GenericRetry<T> WithResult<T>(Func<T> runner)
    {
        return new GenericRetry<T>(runner);
    }

    /// <summary>
    ///     Returns an instance of a fluent retry class with no results/void
    /// </summary>
    public static SimpleRetryAsync WithAsync(Func<Task> runner)
    {
        return new SimpleRetryAsync(runner);
    }

    /// <summary>
    ///     Returns an instance of a fluent retry class with no results/void
    /// </summary>
    public static SimpleRetry With(Action runner)
    {
        return new SimpleRetry(runner);
    }

    /// <summary>
    ///     Sets the default global retry configuration used
    /// </summary>
    /// <exception cref="ArgumentNullException">Throws when <paramref name="retryConfiguration" /> is null</exception>
    public static void SetGlobalRetryConfiguration(RetryConfiguration retryConfiguration)
    {
        RetryConfiguration
            = retryConfiguration ?? throw new ArgumentNullException(nameof(retryConfiguration));
    }
}
