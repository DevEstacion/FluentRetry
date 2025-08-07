namespace FluentRetry;

/// <summary>
/// Extension methods for one-line retry scenarios
/// </summary>
public static class QuickRetryExtensions
{
    /// <summary>
    /// Executes an action with default retry settings (3 attempts, 150ms delay)
    /// </summary>
    public static void WithRetry(this Action action)
    {
        Retry.Do(action).Execute();
    }

    /// <summary>
    /// Executes an action with specified number of attempts
    /// </summary>
    public static void WithRetry(this Action action, int attempts)
    {
        Retry.Do(action).Attempts(attempts).Execute();
    }

    /// <summary>
    /// Executes an async action with default retry settings
    /// </summary>
    public static Task WithRetryAsync(this Func<Task> action)
    {
        return Retry.DoAsync(action).ExecuteAsync();
    }

    /// <summary>
    /// Executes an async action with specified number of attempts
    /// </summary>
    public static Task WithRetryAsync(this Func<Task> action, int attempts)
    {
        return Retry.DoAsync(action).Attempts(attempts).ExecuteAsync();
    }

    /// <summary>
    /// Executes a function with default retry settings
    /// </summary>
    public static T WithRetry<T>(this Func<T> func)
    {
        return Retry.Do(func).Execute();
    }

    /// <summary>
    /// Executes a function with specified number of attempts
    /// </summary>
    public static T WithRetry<T>(this Func<T> func, int attempts)
    {
        return Retry.Do(func).Attempts(attempts).Execute();
    }

    /// <summary>
    /// Executes an async function with default retry settings
    /// </summary>
    public static Task<T> WithRetryAsync<T>(this Func<Task<T>> func)
    {
        return Retry.DoAsync(func).ExecuteAsync();
    }

    /// <summary>
    /// Executes an async function with specified number of attempts
    /// </summary>
    public static Task<T> WithRetryAsync<T>(this Func<Task<T>> func, int attempts)
    {
        return Retry.DoAsync(func).Attempts(attempts).ExecuteAsync();
    }

    /// <summary>
    /// Executes an action with exponential backoff
    /// </summary>
    public static void WithExponentialRetry(this Action action, int attempts = 4, int baseDelayMs = 100)
    {
        Retry.Do(action)
            .Attempts(attempts)
            .Delay(baseDelayMs)
            .WithExponentialBackoff()
            .Execute();
    }

    /// <summary>
    /// Executes an async action with exponential backoff
    /// </summary>
    public static Task WithExponentialRetryAsync(this Func<Task> action, int attempts = 4, int baseDelayMs = 100)
    {
        return Retry.DoAsync(action)
            .Attempts(attempts)
            .Delay(baseDelayMs)
            .WithExponentialBackoff()
            .ExecuteAsync();
    }

    /// <summary>
    /// Executes a function with exponential backoff
    /// </summary>
    public static T WithExponentialRetry<T>(this Func<T> func, int attempts = 4, int baseDelayMs = 100)
    {
        return Retry.Do(func)
            .Attempts(attempts)
            .Delay(baseDelayMs)
            .WithExponentialBackoff()
            .Execute();
    }

    /// <summary>
    /// Executes an async function with exponential backoff
    /// </summary>
    public static Task<T> WithExponentialRetryAsync<T>(this Func<Task<T>> func, int attempts = 4, int baseDelayMs = 100)
    {
        return Retry.DoAsync(func)
            .Attempts(attempts)
            .Delay(baseDelayMs)
            .WithExponentialBackoff()
            .ExecuteAsync();
    }
}
