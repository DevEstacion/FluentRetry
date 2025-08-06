namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryConfiguration
{
    private const int _minRetryCount = 0;
    private const int _maxRetryCount = 100;
    private const int _minRetrySleep = 1;
    private const int _maxRetrySleep = 300000; // 5 minutes

    private readonly int _retryCount;
    private readonly int _retrySleepInMs;
    private readonly Jitter _jitter;

    public RetryConfiguration() : this(3, 150, Jitter.Range(10, 100))
    {
    }

    public RetryConfiguration(int retryCount, int retrySleepInMs, Jitter jitter)
    {
        ValidateRetryCount(retryCount);
        ValidateRetrySleep(retrySleepInMs);
        _retryCount = retryCount;
        _retrySleepInMs = retrySleepInMs;
        _jitter = jitter ?? throw new ArgumentNullException(nameof(jitter));
    }

    /// <summary>
    /// Number of retry attempts. Must be between 0 and 100.
    /// </summary>
    public int RetryCount
    {
        get => _retryCount;
        init
        {
            ValidateRetryCount(value);
            _retryCount = value;
        }
    }

    /// <summary>
    /// Base sleep duration in milliseconds between retries. Must be between 1ms and 300,000ms (5 minutes).
    /// </summary>
    public int RetrySleepInMs
    {
        get => _retrySleepInMs;
        init
        {
            ValidateRetrySleep(value);
            _retrySleepInMs = value;
        }
    }

    /// <summary>
    /// Sets the range of the jitter added to each retry
    /// </summary>
    public Jitter Jitter
    {
        get => _jitter;
        init => _jitter = value ?? throw new ArgumentNullException(nameof(value));
    }

    /// <summary>
    /// Creates a new RetryConfiguration with the specified retry count.
    /// </summary>
    public RetryConfiguration WithRetryCount(int retryCount)
    {
        return new RetryConfiguration(retryCount, _retrySleepInMs, _jitter);
    }

    /// <summary>
    /// Creates a new RetryConfiguration with the specified sleep duration.
    /// </summary>
    public RetryConfiguration WithRetrySleep(int retrySleepInMs)
    {
        return new RetryConfiguration(_retryCount, retrySleepInMs, _jitter);
    }

    /// <summary>
    /// Creates a new RetryConfiguration with the specified jitter.
    /// </summary>
    public RetryConfiguration WithJitter(Jitter jitter)
    {
        return new RetryConfiguration(_retryCount, _retrySleepInMs, jitter);
    }

    private static void ValidateRetryCount(int retryCount)
    {
        if (retryCount < _minRetryCount || retryCount > _maxRetryCount)
        {
            throw new ArgumentOutOfRangeException(nameof(retryCount),
                $"Retry count must be between {_minRetryCount} and {_maxRetryCount}.");
        }
    }

    private static void ValidateRetrySleep(int retrySleepInMs)
    {
        if (retrySleepInMs < _minRetrySleep || retrySleepInMs > _maxRetrySleep)
        {
            throw new ArgumentOutOfRangeException(nameof(retrySleepInMs),
                $"Retry sleep must be between {_minRetrySleep}ms and {_maxRetrySleep}ms.");
        }
    }
}
