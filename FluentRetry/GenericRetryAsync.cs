namespace FluentRetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public class GenericRetryAsync<T> : InternalRetry<GenericRetryAsync<T>>
{
    private readonly Func<Task<T>> _taskRunner;
    private Func<T, bool> _onResultRunner = delegate { return false; };
    private T _result;

    internal GenericRetryAsync(Func<Task<T>> taskRunner)
    {
        _taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
    }

    public GenericRetryAsync<T> WithOnResult(Func<T, bool> onResultRunner)
    {
        _onResultRunner = onResultRunner ?? throw new ArgumentNullException(nameof(onResultRunner));
        return this;
    }

    /// <summary>
    ///     Starts the execution of the initial delegate provided
    /// </summary>
    public async Task<T> Run()
    {
        await Execute();
        return _result;
    }

    protected internal override async Task PerformRunner()
    {
        _result = await _taskRunner();
    }

    protected internal override bool OnResult()
    {
        return _onResultRunner(_result);
    }
}
