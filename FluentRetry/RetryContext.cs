namespace FluentRetry;

public class RetryContext
{
    public Exception Exception { get; init; }
    public int RemainingRetry { get; init; }
}
