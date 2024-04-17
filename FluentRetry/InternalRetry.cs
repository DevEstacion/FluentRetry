// ReSharper disable MemberCanBeProtected.Global

namespace FluentRetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class InternalRetry<TRetry> where TRetry : InternalRetry<TRetry>
{
    internal Action<RetryContext> OnExceptionRunner { get; private set; } = delegate { };
    internal Action<RetryContext> OnFinalExceptionRunner { get; private set; } = delegate { };
    internal RetryConfiguration RetryConfiguration { get; private set; } = Retry.RetryConfiguration;
    internal bool DoublingSleepOnRetry { get; private set; }
    internal bool JitterEnabled { get; private set; } = true;

    public TRetry WithOnException(Action<RetryContext> onExceptionRunner)
    {
        OnExceptionRunner = onExceptionRunner ?? throw new ArgumentNullException(nameof(onExceptionRunner));
        return (TRetry)this;
    }

    public TRetry WithOnFinalException(Action<RetryContext> onExceptionRunner)
    {
        OnFinalExceptionRunner = onExceptionRunner ?? throw new ArgumentNullException(nameof(onExceptionRunner));
        return (TRetry)this;
    }

    public TRetry WithConfiguration(RetryConfiguration retryConfiguration)
    {
        RetryConfiguration = retryConfiguration ?? throw new ArgumentNullException(nameof(retryConfiguration));
        return (TRetry)this;
    }

    public TRetry UseDoublingSleepOnRetry()
    {
        DoublingSleepOnRetry = true;
        return (TRetry)this;
    }

    public TRetry DisableJitter()
    {
        JitterEnabled = false;
        return (TRetry)this;
    }

    protected internal abstract Task PerformRunner();

    protected internal virtual bool OnResult()
    {
        return false;
    }

    protected internal async Task Execute()
    {
        var remainingRetry = RetryConfiguration.RetryCount;
        while (true)
        {
            try
            {
                await PerformRunner();
                var shouldRetry = OnResult();
                if (shouldRetry)
                {
                    throw new Exception("Result was not expected.");
                }

                return;
            }
            catch (Exception ex)
            {
                if (remainingRetry <= 0)
                {
                    OnFinalExceptionRunner.Invoke(new RetryContext
                    { Exception = ex, RemainingRetry = 0, RetrySleepInMs = 0 });
                    throw;
                }

                var totalSleepDelay = GetTotalSleep(remainingRetry);
                await Task.Delay(totalSleepDelay);
                remainingRetry--;

                OnExceptionRunner.Invoke(new RetryContext
                { Exception = ex, RemainingRetry = remainingRetry, RetrySleepInMs = totalSleepDelay });
            }
        }
    }

    private int GetTotalSleep(int remainingRetry)
    {
        var jitter = JitterEnabled ? Random.Shared.Next(10, 100) : 0;
        var totalSleep = RetryConfiguration.RetrySleepInMs;
        if (!DoublingSleepOnRetry)
            return totalSleep + jitter;

        var usedRetry = RetryConfiguration.RetryCount - remainingRetry;
        while (usedRetry > 0)
        {
            totalSleep *= 2;
            usedRetry--;
        }

        return totalSleep + jitter;
    }
}
