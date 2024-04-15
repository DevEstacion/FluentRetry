namespace FluentRetry;

public abstract class InternalRetry<T> where T : InternalRetry<T>
{
    protected Action<Exception> OnExceptionRunner { get; private set; } = delegate { };
    protected Exception[] ExceptionToHandle { get; private set; }
    protected string Caller { get; private set; }
    protected RetryConfiguration RetryConfiguration { get; private set; }


    public T WithOnException(Action<Exception> onExceptionRunner, params Exception[] exceptionToHandle)
    {
        OnExceptionRunner = onExceptionRunner ?? throw new ArgumentNullException(nameof(onExceptionRunner));
        ExceptionToHandle = exceptionToHandle ?? throw new ArgumentNullException(nameof(exceptionToHandle));
        return (T)this;
    }

    public T WithCaller(string caller)
    {
        Caller = string.IsNullOrWhiteSpace(caller)
            ? throw new ArgumentNullException(nameof(caller))
            : caller;
        return (T)this;
    }

    public T WithConfiguration(RetryConfiguration retryConfiguration)
    {
        RetryConfiguration = retryConfiguration ?? throw new ArgumentNullException(nameof(retryConfiguration));
        return (T)this;
    }
}
