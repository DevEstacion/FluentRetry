// ReSharper disable MemberCanBeProtected.Global

namespace FluentRetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class InternalRetry<TRetry> where TRetry : InternalRetry<TRetry>
{
    internal Action<RetryContext> OnExceptionRunner { get; private set; } = static _ => { };
    internal Action<RetryContext> OnFinalExceptionRunner { get; private set; } = static _ => { };
    internal RetryConfiguration RetryConfiguration { get; private set; } = Retry.RetryConfiguration;
    internal bool DoublingSleepOnRetry { get; private set; }
    internal bool JitterEnabled { get; private set; } = true;
    internal bool ShouldThrowOnFinalException { get; private set; }

    /// <summary>
    ///     Adds a delegate to invoke with the exception and retry information every exception
    /// </summary>
    /// <param name="onExceptionRunner">Delegate to execute on exception</param>
    /// <returns>Returns the fluent retry instance</returns>
    /// <exception cref="ArgumentNullException">Throws when <paramref name="onExceptionRunner" /> is null</exception>
    public TRetry WithOnException(Action<RetryContext> onExceptionRunner)
    {
        OnExceptionRunner = onExceptionRunner ?? throw new ArgumentNullException(nameof(onExceptionRunner));
        return (TRetry)this;
    }

    /// <summary>
    ///     Adds a delegate to invoke with the exception and retry information on final exception
    /// </summary>
    /// <param name="onExceptionRunner">Delegate to execute on final exception</param>
    /// <returns>Returns the fluent retry instance</returns>
    /// <exception cref="ArgumentNullException">Throws when <paramref name="onExceptionRunner" /> is null</exception>
    public TRetry WithOnFinalException(Action<RetryContext> onExceptionRunner)
    {
        OnFinalExceptionRunner = onExceptionRunner ?? throw new ArgumentNullException(nameof(onExceptionRunner));
        return (TRetry)this;
    }

    /// <summary>
    ///     Overrides the configuration used during the execution and retry
    /// </summary>
    /// <param name="retryConfiguration">Overrides the <see cref="Retry.RetryConfiguration" /> global configuration</param>
    /// <returns>Returns the fluent retry instance</returns>
    /// <exception cref="ArgumentNullException">Throws when <paramref name="retryConfiguration" /> is null</exception>
    public TRetry WithConfiguration(RetryConfiguration retryConfiguration)
    {
        RetryConfiguration = retryConfiguration ?? throw new ArgumentNullException(nameof(retryConfiguration));
        return (TRetry)this;
    }

    /// <summary>
    ///     Enables exponential backoff where the <see cref="RetryConfiguration.RetrySleepInMs" /> value doubles on each retry
    /// </summary>
    /// <returns>Returns the fluent retry instance</returns>
    public TRetry UseExponentialRetry()
    {
        DoublingSleepOnRetry = true;
        return (TRetry)this;
    }

    /// <summary>
    ///     Uses <paramref name="isEnabled" /> to enable or disable the jitter added to each retry together with
    ///     <see cref="RetryConfiguration.RetrySleepInMs" />
    /// </summary>
    /// <returns>Returns the fluent retry instance</returns>
    public TRetry SetJitterEnabled(bool isEnabled)
    {
        JitterEnabled = isEnabled;
        return (TRetry)this;
    }

    /// <summary>
    ///     Use this to control whether an exception should be thrown on the final retry attempt
    /// </summary>
    /// <param name="isEnabled">True to throw an exception on the final retry, false otherwise</param>
    /// <returns>Returns the fluent retry instance</returns>
    public TRetry ThrowOnFinalException(bool isEnabled)
    {
        ShouldThrowOnFinalException = isEnabled;
        return (TRetry)this;
    }

    protected internal abstract Task PerformRunner();

    protected internal virtual bool OnResult()
    {
        return false;
    }

    protected internal async Task Execute()
    {
        if (RetryConfiguration.RetryCount == 0)
        {
            await ExecuteOnce();
            return;
        }

        await ExecuteWithRetries();
    }

    private async Task ExecuteWithRetries()
    {
        var remainingRetry = RetryConfiguration.RetryCount;

        while (true)
        {
            var result = await TryExecuteOperation();

            if (result.IsSuccess)
                return;

            if (remainingRetry <= 0)
            {
                await HandleFinalFailure(result.Exception);
                return;
            }

            await HandleRetryAttempt(result.Exception, remainingRetry);
            remainingRetry--;
        }
    }

    private async Task<ExecutionResult> TryExecuteOperation()
    {
        try
        {
            await PerformRunner();
            var shouldRetry = OnResult();

            if (shouldRetry)
            {
                var syntheticException = new InvalidOperationException("Result condition not met.");
                return ExecutionResult.Failure(syntheticException);
            }

            return ExecutionResult.Success();
        }
        catch (Exception ex)
        {
            return ExecutionResult.Failure(ex);
        }
    }

    private async Task HandleRetryAttempt(Exception exception, int remainingRetry)
    {
        var totalSleepDelay = GetTotalSleep(remainingRetry);
        var currentAttempt = RetryConfiguration.RetryCount - remainingRetry + 1;

        var context = new RetryContext
        {
            Exception = exception,
            RemainingRetry = remainingRetry,
            RetrySleepInMs = totalSleepDelay,
            AttemptNumber = currentAttempt
        };

        OnExceptionRunner.Invoke(context);
        await Task.Delay(totalSleepDelay);
    }

    private async Task HandleFinalFailure(Exception exception)
    {
        var finalContext = new RetryContext
        {
            Exception = exception,
            RemainingRetry = 0,
            RetrySleepInMs = 0,
            AttemptNumber = RetryConfiguration.RetryCount + 1
        };

        OnFinalExceptionRunner.Invoke(finalContext);

        if (ShouldThrowOnFinalException)
            throw exception;
    }

    private readonly struct ExecutionResult
    {
        public bool IsSuccess { get; }
        public Exception Exception { get; }

        private ExecutionResult(bool isSuccess, Exception exception)
        {
            IsSuccess = isSuccess;
            Exception = exception;
        }

        public static ExecutionResult Success() => new(true, null);
        public static ExecutionResult Failure(Exception exception) => new(false, exception);
    }

    private async Task ExecuteOnce()
    {
        try
        {
            await PerformRunner();
            var shouldRetry = OnResult();
            if (shouldRetry)
            {
                throw new InvalidOperationException("Result condition not met and no retries configured.");
            }
        }
        catch (Exception ex)
        {
            var context = new RetryContext
            {
                Exception = ex,
                RemainingRetry = 0,
                RetrySleepInMs = 0,
                AttemptNumber = 1
            };

            OnFinalExceptionRunner.Invoke(context);

            if (ShouldThrowOnFinalException)
                throw;
        }
    }

    private int GetTotalSleep(int remainingRetry)
    {
        var jitter = JitterEnabled ? RetryConfiguration.Jitter.GetJitter() : 0;
        var baseSleep = RetryConfiguration.RetrySleepInMs;

        if (!DoublingSleepOnRetry)
            return baseSleep + jitter;

        // Calculate exponential backoff more efficiently
        var attemptNumber = RetryConfiguration.RetryCount - remainingRetry + 1;
        var exponentialSleep = baseSleep << Math.Min(attemptNumber - 1, 20); // Cap to prevent overflow

        // Prevent integer overflow
        return exponentialSleep > 0 ? exponentialSleep + jitter : int.MaxValue;
    }
}
