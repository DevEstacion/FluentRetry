namespace FluentRetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public class SimpleRetry : InternalRetry<SimpleRetry>
{
    private readonly Action _actionRunner;

    internal SimpleRetry(Action actionRunner)
    {
        _actionRunner = actionRunner ?? throw new ArgumentNullException(nameof(actionRunner));
    }

    /// <summary>
    ///     Starts the execution of the initial delegate provided
    /// </summary>
    public void Run()
    {
        Execute().GetAwaiter().GetResult();
    }

    protected internal override Task PerformRunner()
    {
        _actionRunner();
        return Task.CompletedTask;
    }
}
