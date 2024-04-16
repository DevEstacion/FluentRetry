using System.Collections.Generic;
using System.Linq;

// ReSharper disable MemberCanBeProtected.Global


namespace FluentRetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public abstract class InternalRetry<TRetry> where TRetry : InternalRetry<TRetry>
{
    internal Action<RetryContext> OnExceptionRunner { get; private set; } = delegate { };
    internal HashSet<Type> ExceptionToHandle { get; private set; } = new(new[] {typeof(Exception)});
    internal string Caller { get; private set; } = "InternalRetry";
    internal RetryConfiguration RetryConfiguration { get; private set; } = RetryInternals.RetryConfiguration;

    public TRetry WithOnException(Action<RetryContext> onExceptionRunner, params Type[] exceptionToHandle)
    {
        OnExceptionRunner = onExceptionRunner ?? throw new ArgumentNullException(nameof(onExceptionRunner));
        ExceptionToHandle = exceptionToHandle == null
            ? throw new ArgumentNullException(nameof(exceptionToHandle))
            : new HashSet<Type>(exceptionToHandle == Array.Empty<Type>()
                ? new[] {typeof(Exception)}
                : exceptionToHandle.Distinct());
        return (TRetry)this;
    }

    public TRetry WithCaller(string caller)
    {
        Caller = string.IsNullOrWhiteSpace(caller)
            ? throw new ArgumentNullException(nameof(caller))
            : caller;
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
                var message
                    = $"An exception occured during {Caller}. Remaining Retry: {totalRetry}. Message: {ex.Message}, Trace: {ex.StackTrace}";
                if ((!ExceptionToHandle.Contains(typeof(Exception)) && !ExceptionToHandle.Contains(ex.GetType()))
                    || totalRetry <= 0)
                {
                    RetryConfiguration.LogHandler.Invoke(new RetryLog
                        {Exception = ex, Message = message, Type = RetryLogType.Exception});
                    throw;
                }

                var totalRetryDelay = RetryConfiguration.RetrySleepInMs + Random.Shared.Next(10, 100);
                RetryConfiguration.LogHandler.Invoke(new RetryLog
                {
                    Exception = ex, Message = $"{message}, Sleep for {totalRetryDelay}ms", Type = RetryLogType.Warning
                });
                await Task.Delay(totalRetryDelay);
                totalRetry--;

                OnExceptionRunner.Invoke(new RetryContext {Exception = ex, RemainingRetry = totalRetry});
            }
        }
    }
}
