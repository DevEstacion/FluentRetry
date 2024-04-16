// ReSharper disable MemberCanBeProtected.Global

namespace FluentRetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class InternalRetry<TRetry> where TRetry : InternalRetry<TRetry>
{
    internal Action<RetryContext> OnExceptionRunner { get; private set; } = delegate { };
    internal Action<RetryContext> OnFinalExceptionRunner { get; private set; } = delegate { };
    internal RetryConfiguration RetryConfiguration { get; private set; } = Retry.RetryConfiguration;

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

    protected internal abstract Task PerformRunner();

    protected internal virtual bool OnResult()
    {
        return false;
    }

    protected internal async Task Execute()
    {
        var totalRetry = RetryConfiguration.RetryCount;
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
                if (totalRetry <= 0)
                {
                    OnFinalExceptionRunner.Invoke(new RetryContext
                        { Exception = ex, RemainingRetry = 0, RetrySleetInMs = 0 });
                    throw;
                }

                var totalRetryDelay = RetryConfiguration.RetrySleepInMs + Random.Shared.Next(10, 100);
                await Task.Delay(totalRetryDelay);
                totalRetry--;

                OnExceptionRunner.Invoke(new RetryContext
                    { Exception = ex, RemainingRetry = totalRetry, RetrySleetInMs = totalRetryDelay });
            }
        }
    }
}
