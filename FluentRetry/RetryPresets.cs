namespace FluentRetry;

/// <summary>
/// Predefined retry configurations for common scenarios
/// </summary>
[ExcludeFromCodeCoverage]
public static class RetryPresets
{
    /// <summary>
    /// Fast operations - minimal delays (2 attempts, 50ms delay, 25ms jitter)
    /// </summary>
    public static RetryBuilder Fast(this RetryBuilder builder)
    {
        return builder.Attempts(2).Delay(50).WithJitter(25);
    }

    /// <summary>
    /// Standard operations - balanced settings (3 attempts, 150ms delay, 50ms jitter)
    /// </summary>
    public static RetryBuilder Standard(this RetryBuilder builder)
    {
        return builder.Attempts(3).Delay(150).WithJitter(50);
    }

    /// <summary>
    /// Resilient operations - more retries for unreliable services (5 attempts, 500ms delay, 200ms jitter)
    /// </summary>
    public static RetryBuilder Resilient(this RetryBuilder builder)
    {
        return builder.Attempts(5).Delay(500).WithJitter(200);
    }

    /// <summary>
    /// Network operations - exponential backoff for HTTP calls (4 attempts, 100ms delay, exponential backoff)
    /// </summary>
    public static RetryBuilder Network(this RetryBuilder builder)
    {
        return builder.Attempts(4).Delay(100).WithExponentialBackoff().WithJitter(50);
    }

    /// <summary>
    /// Database operations - longer delays for database timeouts (originally 1000ms/200ms jitter).
    /// Reduced to 300ms delay & 100ms jitter to speed up CI while retaining a slower profile than Standard.
    /// </summary>
    public static RetryBuilder Database(this RetryBuilder builder)
    {
        return builder.Attempts(3).Delay(300).WithJitter(100);
    }

    /// <summary>
    /// Fast operations - minimal delays (2 attempts, 50ms delay, 25ms jitter)
    /// </summary>
    public static RetryBuilder<T> Fast<T>(this RetryBuilder<T> builder)
    {
        return builder.Attempts(2).Delay(50).WithJitter(25);
    }

    /// <summary>
    /// Standard operations - balanced settings (3 attempts, 150ms delay, 50ms jitter)
    /// </summary>
    public static RetryBuilder<T> Standard<T>(this RetryBuilder<T> builder)
    {
        return builder.Attempts(3).Delay(150).WithJitter(50);
    }

    /// <summary>
    /// Resilient operations - more retries for unreliable services (5 attempts, 500ms delay, 200ms jitter)
    /// </summary>
    public static RetryBuilder<T> Resilient<T>(this RetryBuilder<T> builder)
    {
        return builder.Attempts(5).Delay(500).WithJitter(200);
    }

    /// <summary>
    /// Network operations - exponential backoff for HTTP calls (4 attempts, 100ms delay, exponential backoff)
    /// </summary>
    public static RetryBuilder<T> Network<T>(this RetryBuilder<T> builder)
    {
        return builder.Attempts(4).Delay(100).WithExponentialBackoff().WithJitter(50);
    }

    /// <summary>
    /// Database operations - longer delays for database timeouts (originally 1000ms/200ms jitter).
    /// Reduced to 300ms delay & 100ms jitter to speed up CI while retaining a slower profile than Standard.
    /// </summary>
    public static RetryBuilder<T> Database<T>(this RetryBuilder<T> builder)
    {
        return builder.Attempts(3).Delay(300).WithJitter(100);
    }
}
