using System;
using System.Threading.Tasks;

namespace FluentRetry;

public class Retry<T>
{
    private readonly Task<T> _runner;
    private Action<Exception> _onExceptionRunner = delegate { };
    private Exception[] _exceptionToHandle;
    private Action<T> _onResultRunner = delegate { };
    private string _caller;
    private RetryConfiguration _retryConfiguration;

    private Retry(Task<T> runner)
    {
        _runner = runner ?? throw new ArgumentNullException(nameof(runner));
    }

    public Retry<T> WithOnResult(Action<T> onResultRunner)
    {
        _onResultRunner = onResultRunner ?? throw new ArgumentNullException(nameof(onResultRunner));
        return this;
    }

    public Retry<T> WithOnException(Action<Exception> onExceptionRunner, params Exception[] exceptionToHandle)
    {
        _onExceptionRunner = onExceptionRunner ?? throw new ArgumentNullException(nameof(onExceptionRunner));
        _exceptionToHandle = exceptionToHandle ?? throw new ArgumentNullException(nameof(exceptionToHandle));
        return this;
    }

    public Retry<T> WithCaller(string caller)
    {
        _caller = string.IsNullOrWhiteSpace(caller)
            ? throw new ArgumentNullException(nameof(caller))
            : caller;
        return this;
    }

    public Retry<T> WithConfiguration(RetryConfiguration retryConfiguration)
    {
        _retryConfiguration = retryConfiguration ?? throw new ArgumentNullException(nameof(retryConfiguration));
        return this;
    }
}
