namespace FluentRetry;

/// <summary>
/// Extension methods for common retry scenarios.
/// </summary>
public static class RetryExtensions
{
    /// <summary>
    /// Executes an action with retry logic using the default configuration.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="retryConfiguration">Optional retry configuration. Uses global default if not provided.</param>
    public static void WithRetry(this Action action, RetryConfiguration retryConfiguration = null)
    {
        var retry = Retry.With(action);
        if (retryConfiguration != null)
            retry.WithConfiguration(retryConfiguration);
        retry.Run();
    }

    /// <summary>
    /// Executes an async action with retry logic using the default configuration.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <param name="retryConfiguration">Optional retry configuration. Uses global default if not provided.</param>
    public static async Task WithRetryAsync(this Func<Task> action, RetryConfiguration retryConfiguration = null)
    {
        var retry = Retry.WithAsync(action);
        if (retryConfiguration != null)
            retry.WithConfiguration(retryConfiguration);
        await retry.Run();
    }

    /// <summary>
    /// Executes a function with retry logic using the default configuration.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="retryConfiguration">Optional retry configuration. Uses global default if not provided.</param>
    /// <returns>The result of the function.</returns>
    public static T WithRetry<T>(this Func<T> func, RetryConfiguration retryConfiguration = null)
    {
        var retry = Retry.WithResult(func);
        if (retryConfiguration != null)
            retry.WithConfiguration(retryConfiguration);
        return retry.Run();
    }

    /// <summary>
    /// Executes an async function with retry logic using the default configuration.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <param name="retryConfiguration">Optional retry configuration. Uses global default if not provided.</param>
    /// <returns>The result of the function.</returns>
    public static async Task<T> WithRetryAsync<T>(this Func<Task<T>> func, RetryConfiguration retryConfiguration = null)
    {
        var retry = Retry.WithResultAsync(func);
        if (retryConfiguration != null)
            retry.WithConfiguration(retryConfiguration);
        return await retry.Run();
    }

    /// <summary>
    /// Executes an action with exponential backoff retry logic.
    /// </summary>
    /// <param name="action">The action to execute.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="baseDelayMs">Base delay in milliseconds.</param>
    public static void WithExponentialRetry(this Action action, int maxRetries = 3, int baseDelayMs = 100)
    {
        Retry.With(action)
            .WithConfiguration(RetryPolicies.ExponentialBackoff(maxRetries, baseDelayMs))
            .UseExponentialRetry()
            .Run();
    }

    /// <summary>
    /// Executes an async action with exponential backoff retry logic.
    /// </summary>
    /// <param name="action">The async action to execute.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="baseDelayMs">Base delay in milliseconds.</param>
    public static async Task WithExponentialRetryAsync(this Func<Task> action, int maxRetries = 3, int baseDelayMs = 100)
    {
        await Retry.WithAsync(action)
            .WithConfiguration(RetryPolicies.ExponentialBackoff(maxRetries, baseDelayMs))
            .UseExponentialRetry()
            .Run();
    }

    /// <summary>
    /// Executes a function with exponential backoff retry logic.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The function to execute.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="baseDelayMs">Base delay in milliseconds.</param>
    /// <returns>The result of the function.</returns>
    public static T WithExponentialRetry<T>(this Func<T> func, int maxRetries = 3, int baseDelayMs = 100)
    {
        return Retry.WithResult(func)
            .WithConfiguration(RetryPolicies.ExponentialBackoff(maxRetries, baseDelayMs))
            .UseExponentialRetry()
            .Run();
    }

    /// <summary>
    /// Executes an async function with exponential backoff retry logic.
    /// </summary>
    /// <typeparam name="T">The return type of the function.</typeparam>
    /// <param name="func">The async function to execute.</param>
    /// <param name="maxRetries">Maximum number of retry attempts.</param>
    /// <param name="baseDelayMs">Base delay in milliseconds.</param>
    /// <returns>The result of the function.</returns>
    public static async Task<T> WithExponentialRetryAsync<T>(this Func<Task<T>> func, int maxRetries = 3, int baseDelayMs = 100)
    {
        return await Retry.WithResultAsync(func)
            .WithConfiguration(RetryPolicies.ExponentialBackoff(maxRetries, baseDelayMs))
            .UseExponentialRetry()
            .Run();
    }
}
