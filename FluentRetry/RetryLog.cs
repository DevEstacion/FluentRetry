namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryLog
{
    internal RetryLog()
    {
    }

    public string Message { get; init; }
    public RetryLogType Type { get; init; }
    public Exception Exception { get; init; }
}
