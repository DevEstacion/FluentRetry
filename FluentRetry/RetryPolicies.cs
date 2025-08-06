namespace FluentRetry;

/// <summary>
/// Provides predefined retry policies for common scenarios.
/// </summary>
public static class RetryPolicies
{
    /// <summary>
    /// A conservative retry policy with minimal delays - good for fast operations.
    /// </summary>
    public static RetryConfiguration Conservative => new(
        retryCount: 2,
        retrySleepInMs: 50,
        jitter: Jitter.UpTo(25)
    );

    /// <summary>
    /// A moderate retry policy with balanced delays - good for most scenarios.
    /// </summary>
    public static RetryConfiguration Moderate => new(
        retryCount: 3,
        retrySleepInMs: 150,
        jitter: Jitter.Range(10, 100)
    );

    /// <summary>
    /// An aggressive retry policy with more attempts and longer delays - good for unreliable services.
    /// </summary>
    public static RetryConfiguration Aggressive => new(
        retryCount: 5,
        retrySleepInMs: 500,
        jitter: Jitter.Range(50, 200)
    );

    /// <summary>
    /// A network-optimized retry policy with exponential backoff suitable for HTTP calls.
    /// </summary>
    public static RetryConfiguration Network => new(
        retryCount: 4,
        retrySleepInMs: 100,
        jitter: Jitter.Percentage(25, 100)
    );

    /// <summary>
    /// A database-optimized retry policy with longer delays for database operations.
    /// </summary>
    public static RetryConfiguration Database => new(
        retryCount: 3,
        retrySleepInMs: 1000,
        jitter: Jitter.Range(100, 500)
    );

    /// <summary>
    /// A file I/O optimized retry policy with quick retries for transient file locks.
    /// </summary>
    public static RetryConfiguration FileIO => new(
        retryCount: 5,
        retrySleepInMs: 25,
        jitter: Jitter.UpTo(15)
    );

    /// <summary>
    /// Creates a custom exponential backoff policy.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="baseDelayMs">Base delay in milliseconds.</param>
    /// <param name="jitterPercentage">Jitter as percentage of base delay (0-100).</param>
    /// <returns>A configured RetryConfiguration for exponential backoff.</returns>
    public static RetryConfiguration ExponentialBackoff(int maxRetries = 3, int baseDelayMs = 100, double jitterPercentage = 20)
    {
        return new RetryConfiguration(
            retryCount: maxRetries,
            retrySleepInMs: baseDelayMs,
            jitter: Jitter.Percentage(jitterPercentage, baseDelayMs)
        );
    }

    /// <summary>
    /// Creates a linear backoff policy where delay increases linearly.
    /// </summary>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="baseDelayMs">Base delay in milliseconds.</param>
    /// <param name="incrementMs">Amount to increase delay by on each retry.</param>
    /// <returns>A configured RetryConfiguration for linear backoff.</returns>
    public static RetryConfiguration LinearBackoff(int maxRetries = 3, int baseDelayMs = 100, int incrementMs = 100)
    {
        return new RetryConfiguration(
            retryCount: maxRetries,
            retrySleepInMs: baseDelayMs,
            jitter: Jitter.UpTo(incrementMs)
        );
    }
}
