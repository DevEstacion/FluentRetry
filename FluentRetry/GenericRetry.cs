namespace FluentRetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public class GenericRetry<T> : InternalRetry<GenericRetry<T>>
{
    private readonly Func<T> _funcRunner;
    private Func<T, bool> _onResultRunner = delegate { return false; };
    private T _result;

    internal GenericRetry(Func<T> funcRunner)
    {
        _funcRunner = funcRunner ?? throw new ArgumentNullException(nameof(funcRunner));
    }

    /// <summary>
    ///     Retries when <paramref name="onResultRunner"/> returns <c>true</c>
    /// </summary>
    public GenericRetry<T> WithOnResult(Func<T, bool> onResultRunner)
    {
        _onResultRunner = onResultRunner ?? throw new ArgumentNullException(nameof(onResultRunner));
        return this;
    }

    /// <summary>
    ///     Starts the execution of the initial delegate provided
    /// </summary>
    public T Run()
    {
        Execute().GetAwaiter().GetResult();
        return _result;
    }

    protected internal override Task PerformRunner()
    {
        _result = _funcRunner();
        return Task.CompletedTask;
    }

    protected internal override bool OnResult()
    {
        return _onResultRunner(_result);
    }
}
