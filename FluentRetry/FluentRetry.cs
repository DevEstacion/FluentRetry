namespace FluentRetry;

/// <summary>
/// Simplified retry builder for operations without return values
/// </summary>
[ExcludeFromCodeCoverage]
public class RetryBuilder
{
    private readonly Func<Task<object>> _operation;
    private int _maxAttempts;
    private int _delayMs;
    private bool _useExponentialBackoff;
    private int _maxJitterMs = 50;
    private Action<Exception, int> _onRetry = (_, _) => { };
    private Action<Exception> _onFailure = _ => { };
    private bool _throwOnFailure;

    internal RetryBuilder(Func<Task<object>> operation)
    {
        _operation = operation ?? throw new ArgumentNullException(nameof(operation));
        var (attempts, delayMs) = Retry.GetDefaults();
        _maxAttempts = attempts;
        _delayMs = delayMs;
    }

    /// <summary>
    /// Sets the maximum number of retry attempts (default: 3)
    /// </summary>
    public RetryBuilder Attempts(int maxAttempts)
    {
        _maxAttempts = Math.Max(1, maxAttempts);
        return this;
    }

    /// <summary>
    /// Sets the delay between retries in milliseconds (default: 150ms)
    /// </summary>
    public RetryBuilder Delay(int milliseconds)
    {
        _delayMs = Math.Max(0, milliseconds);
        return this;
    }

    /// <summary>
    /// Enables exponential backoff (doubles delay on each retry)
    /// </summary>
    public RetryBuilder WithExponentialBackoff(bool enabled = true)
    {
        _useExponentialBackoff = enabled;
        return this;
    }

    /// <summary>
    /// Sets maximum jitter to add randomness to delays (default: 50ms)
    /// </summary>
    public RetryBuilder WithJitter(int maxJitterMs)
    {
        _maxJitterMs = Math.Max(0, maxJitterMs);
        return this;
    }

    /// <summary>
    /// Called on each retry attempt with the exception and attempt number
    /// </summary>
    public RetryBuilder OnRetry(Action<Exception, int> callback)
    {
        _onRetry = callback ?? ((_, _) => { });
        return this;
    }

    /// <summary>
    /// Called when all retries are exhausted
    /// </summary>
    public RetryBuilder OnFailure(Action<Exception> callback)
    {
        _onFailure = callback ?? (_ => { });
        return this;
    }

    /// <summary>
    /// Whether to throw the final exception if all retries fail (default: false)
    /// </summary>
    public RetryBuilder ThrowOnFailure(bool enabled = true)
    {
        _throwOnFailure = enabled;
        return this;
    }

    /// <summary>
    /// Executes the operation with retry logic
    /// </summary>
    public void Execute()
    {
        ExecuteAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes the operation with retry logic asynchronously
    /// </summary>
    public async Task ExecuteAsync()
    {
        Exception lastException = null;

        for (var attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                await _operation();
                return; // Success
            }
            catch (OperationCanceledException)
            {
                // Don't retry on cancellation, immediately propagate
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == _maxAttempts)
                    break; // Final attempt failed

                _onRetry(ex, attempt);
                await DelayBeforeRetry(attempt);
            }
        }

        _onFailure?.Invoke(lastException);

        if (_throwOnFailure && lastException != null)
            throw lastException;
    }

    private async Task DelayBeforeRetry(int attempt)
    {
        if (_delayMs <= 0)
            return;

        var delay = _delayMs;

        if (_useExponentialBackoff)
            delay = (int)Math.Min(delay * Math.Pow(2, attempt - 1), 30000); // Cap at 30 seconds

        if (_maxJitterMs > 0)
            delay += Random.Shared.Next(0, _maxJitterMs + 1);

        await Task.Delay(delay);
    }
}

/// <summary>
/// Simplified retry builder for operations that return values
/// </summary>
[ExcludeFromCodeCoverage]
public class RetryBuilder<T>
{
    private readonly Func<Task<T>> _operation;
    private int _maxAttempts;
    private int _delayMs;
    private bool _useExponentialBackoff;
    private int _maxJitterMs = 50;
    private Func<T, bool> _retryCondition = _ => false;
    private Action<Exception, int> _onRetry = (_, _) => { };
    private Action<Exception> _onFailure = _ => { };
    private bool _throwOnFailure;

    internal RetryBuilder(Func<Task<T>> operation)
    {
        _operation = operation ?? throw new ArgumentNullException(nameof(operation));
        var (attempts, delayMs) = Retry.GetDefaults();
        _maxAttempts = attempts;
        _delayMs = delayMs;
    }

    /// <summary>
    /// Sets the maximum number of retry attempts (default: 3)
    /// </summary>
    public RetryBuilder<T> Attempts(int maxAttempts)
    {
        _maxAttempts = Math.Max(1, maxAttempts);
        return this;
    }

    /// <summary>
    /// Sets the delay between retries in milliseconds (default: 150ms)
    /// </summary>
    public RetryBuilder<T> Delay(int milliseconds)
    {
        _delayMs = Math.Max(0, milliseconds);
        return this;
    }

    /// <summary>
    /// Enables exponential backoff (doubles delay on each retry)
    /// </summary>
    public RetryBuilder<T> WithExponentialBackoff(bool enabled = true)
    {
        _useExponentialBackoff = enabled;
        return this;
    }

    /// <summary>
    /// Sets maximum jitter to add randomness to delays (default: 50ms)
    /// </summary>
    public RetryBuilder<T> WithJitter(int maxJitterMs)
    {
        _maxJitterMs = Math.Max(0, maxJitterMs);
        return this;
    }

    /// <summary>
    /// Retries when the result matches the condition
    /// </summary>
    public RetryBuilder<T> RetryWhen(Func<T, bool> condition)
    {
        _retryCondition = condition ?? (_ => false);
        return this;
    }

    /// <summary>
    /// Called on each retry attempt with the exception and attempt number
    /// </summary>
    public RetryBuilder<T> OnRetry(Action<Exception, int> callback)
    {
        _onRetry = callback ?? ((_, _) => { });
        return this;
    }

    /// <summary>
    /// Called when all retries are exhausted
    /// </summary>
    public RetryBuilder<T> OnFailure(Action<Exception> callback)
    {
        _onFailure = callback ?? (_ => { });
        return this;
    }

    /// <summary>
    /// Whether to throw the final exception if all retries fail (default: false)
    /// </summary>
    public RetryBuilder<T> ThrowOnFailure(bool enabled = true)
    {
        _throwOnFailure = enabled;
        return this;
    }

    /// <summary>
    /// Executes the operation with retry logic
    /// </summary>
    public T Execute()
    {
        return ExecuteAsync().GetAwaiter().GetResult();
    }

    /// <summary>
    /// Executes the operation with retry logic asynchronously
    /// </summary>
    public async Task<T> ExecuteAsync()
    {
        Exception lastException = null;
        T lastResult = default;

        for (var attempt = 1; attempt <= _maxAttempts; attempt++)
        {
            try
            {
                lastResult = await _operation();

                // Check if we should retry based on result
                if (!_retryCondition(lastResult))
                    return lastResult; // Success

                // If this is the last attempt and we're retrying based on result, break
                if (attempt == _maxAttempts)
                {
                    lastException = new InvalidOperationException("Retry condition was not satisfied after all attempts");
                    break;
                }

                // Create a fake exception for retry condition
                lastException = new InvalidOperationException($"Retry condition not met on attempt {attempt}");
                _onRetry(lastException, attempt);
                await DelayBeforeRetry(attempt);
            }
            catch (OperationCanceledException)
            {
                // Don't retry on cancellation, immediately propagate
                throw;
            }
            catch (Exception ex)
            {
                lastException = ex;

                if (attempt == _maxAttempts)
                    break; // Final attempt failed

                _onRetry(ex, attempt);
                await DelayBeforeRetry(attempt);
            }
        }

        _onFailure?.Invoke(lastException);

        if (_throwOnFailure && lastException != null)
            throw lastException;

        return lastResult;
    }

    private async Task DelayBeforeRetry(int attempt)
    {
        if (_delayMs <= 0)
            return;

        var delay = _delayMs;

        if (_useExponentialBackoff)
            delay = (int)Math.Min(delay * Math.Pow(2, attempt - 1), 30000); // Cap at 30 seconds

        if (_maxJitterMs > 0)
            delay += Random.Shared.Next(0, _maxJitterMs + 1);

        await Task.Delay(delay);
    }
}
