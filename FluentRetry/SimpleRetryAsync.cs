namespace FluentRetry;

[EditorBrowsable(EditorBrowsableState.Never)]
public class SimpleRetryAsync : InternalRetry<SimpleRetryAsync>
{
    private readonly Func<Task> _taskRunner;

    internal SimpleRetryAsync(Func<Task> taskRunner)
    {
        _taskRunner = taskRunner ?? throw new ArgumentNullException(nameof(taskRunner));
    }

    public Task Run()
    {
        return Execute();
    }

    protected internal override Task PerformRunner()
    {
        return _taskRunner();
    }
}
