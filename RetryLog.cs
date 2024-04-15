using System;

namespace FluentRetry;

public class RetryLog
{
    public string Message { get; init; }
    public RetryLogType Type { get; init; }
    public Exception Exception { get; init; }
}
