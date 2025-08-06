namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryContext
{
    internal RetryContext()
    {
        Timestamp = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// The exception that caused the retry attempt.
    /// </summary>
    public Exception Exception { get; init; } = null!;

    /// <summary>
    /// Number of retry attempts remaining after this attempt.
    /// </summary>
    public int RemainingRetry { get; init; }

    /// <summary>
    /// The delay in milliseconds before the next retry attempt.
    /// </summary>
    public int RetrySleepInMs { get; init; }

    /// <summary>
    /// The current attempt number (1-based).
    /// </summary>
    public int AttemptNumber { get; init; }

    /// <summary>
    /// The timestamp when this retry context was created.
    /// </summary>
    public DateTimeOffset Timestamp { get; }

    /// <summary>
    /// Total number of retry attempts configured.
    /// </summary>
    public int TotalRetryCount => AttemptNumber + RemainingRetry - 1;

    /// <summary>
    /// Indicates whether this is the final attempt.
    /// </summary>
    public bool IsFinalAttempt => RemainingRetry == 0;

    /// <summary>
    /// Gets the exception message or a default message if exception is null.
    /// </summary>
    public string ExceptionMessage => Exception?.Message ?? "No exception";

    /// <summary>
    /// Gets the exception type name or "Unknown" if exception is null.
    /// </summary>
    public string ExceptionType => Exception?.GetType().Name ?? "Unknown";

    public override string ToString()
    {
        return $"Attempt {AttemptNumber}/{TotalRetryCount + 1}: {ExceptionType} - {ExceptionMessage}" +
               (IsFinalAttempt ? " (Final)" : $" (Next delay: {RetrySleepInMs}ms)");
    }
}
