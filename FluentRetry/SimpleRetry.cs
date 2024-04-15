namespace FluentRetry;

public class SimpleRetry : InternalRetry<SimpleRetry>
{
    private readonly Task _taskRunner;
    private readonly Action _actionRunner;

    internal SimpleRetry(Task taskRunner)
    {
        _taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
    }

    internal SimpleRetry(Action actionRunner)
    {
        _actionRunner = actionRunner ?? throw new ArgumentNullException(nameof(actionRunner));
    }

    private bool IsAsync => _taskRunner != null;
}

public class GenericRetry<T> : InternalRetry<GenericRetry<T>>
{
    private readonly Task<T> _taskRunner;
    private readonly Func<T> _funcRunner;
    private Func<T, bool> _onResultRunner = delegate { return false; };

    internal GenericRetry(Task<T> taskRunner)
    {
        _taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
    }

    internal GenericRetry(Func<T> funcRunner)
    {
        _funcRunner = funcRunner ?? throw new ArgumentNullException(nameof(funcRunner));
    }

    private bool IsAsync => _taskRunner != null;

    public GenericRetry<T> WithOnResult(Func<T, bool> onResultRunner)
    {
        _onResultRunner = onResultRunner ?? throw new ArgumentNullException(nameof(onResultRunner));
        return this;
    }
}
