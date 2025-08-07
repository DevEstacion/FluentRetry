using System.Diagnostics.CodeAnalysis;

namespace FluentRetry;

/// <summary>
/// Simple and fluent retry implementation for all scenarios
/// </summary>
[ExcludeFromCodeCoverage]
public static class Retry
{
    private static int _defaultAttempts = 3;
    private static int _defaultDelayMs = 150;

    /// <summary>
    /// Creates a retry operation for any operation (action or function)
    /// </summary>
    public static RetryBuilder Do(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new RetryBuilder(() =>
        {
            action();
            return Task.FromResult<object>(null);
        });
    }

    /// <summary>
    /// Creates a retry operation for any async operation
    /// </summary>
    public static RetryBuilder DoAsync(Func<Task> action)
    {
        ArgumentNullException.ThrowIfNull(action);
        return new RetryBuilder(async () =>
        {
            await action();
            return null;
        });
    }

    /// <summary>
    /// Creates a retry operation for a function that returns a value
    /// </summary>
    public static RetryBuilder<T> Do<T>(Func<T> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return new RetryBuilder<T>(() => Task.FromResult(func()));
    }

    /// <summary>
    /// Creates a retry operation for an async function that returns a value
    /// </summary>
    public static RetryBuilder<T> DoAsync<T>(Func<Task<T>> func)
    {
        ArgumentNullException.ThrowIfNull(func);
        return new RetryBuilder<T>(func);
    }

    /// <summary>
    /// Sets global defaults for all retry operations
    /// </summary>
    public static void SetGlobalDefaults(int attempts = 3, int delayMs = 150)
    {
        _defaultAttempts = Math.Max(1, attempts);
        _defaultDelayMs = Math.Max(0, delayMs);
    }

    internal static (int attempts, int delayMs) GetDefaults() => (_defaultAttempts, _defaultDelayMs);
}
