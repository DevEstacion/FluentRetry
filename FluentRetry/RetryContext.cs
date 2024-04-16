namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryContext
{
    internal RetryContext()
    {
    }

    public Exception Exception { get; init; }
    public int RemainingRetry { get; init; }
    public int RetrySleetInMs { get; init; }
}
