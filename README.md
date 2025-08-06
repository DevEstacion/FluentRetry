# FluentRetry

A lightweight, high-performance, and fluent retry implementation for C# with advanced features like exponential backoff, jitter strategies, predefined policies, and comprehensive observability.

## Features

✨ **Simple & Fluent API** - Easy-to-use fluent interface for all retry scenarios  
🚀 **High Performance** - Optimized retry logic with minimal allocations  
📊 **Rich Observability** - Detailed retry context with timestamps and attempt tracking  
🎯 **Multiple Jitter Strategies** - Range, percentage-based, and full jitter support  
📋 **Predefined Policies** - Ready-to-use policies for common scenarios  
🔧 **Flexible Configuration** - Comprehensive configuration with validation  
🛡️ **Production Ready** - Overflow protection and robust error handling  

## Installation

You can get the package from [nuget.org](https://www.nuget.org/packages/FluentRetry) or run the following from your terminal.

```bash
dotnet add package FluentRetry
```

## Quick Start

Add the using declaration:

```csharp
using FluentRetry;
```

### Basic Usage

```csharp
// Simple retry with default configuration (3 retries, 150ms delay)
Retry.With(() => SomeOperation()).Run();

// Async operation
await Retry.WithAsync(async () => await SomeAsyncOperation()).Run();

// With return value
var result = Retry.WithResult(() => GetSomeValue()).Run();

// Async with return value
var result = await Retry.WithResultAsync(async () => await GetSomeValueAsync()).Run();
```

### Using Extension Methods

```csharp
// Simple extension method usage
SomeOperation.WithRetry();

// Async with custom configuration
await SomeAsyncOperation.WithRetryAsync(RetryPolicies.Network);

// Exponential backoff
var result = SomeFunction.WithExponentialRetry(maxRetries: 5, baseDelayMs: 200);
```

## API Reference

### Core Methods

```csharp
Retry.With(Action action)                    // Sync action without return value
Retry.WithAsync(Func<Task> action)           // Async action without return value
Retry.WithResult<T>(Func<T> func)            // Sync function with return value
Retry.WithResultAsync<T>(Func<Task<T>> func) // Async function with return value
Retry.SetGlobalRetryConfiguration(config)   // Set global default configuration
```

## Configuration

### Retry Configuration

```csharp
var config = new RetryConfiguration
{
    RetryCount = 5,           // Number of retry attempts (0-100)
    RetrySleepInMs = 1000,    // Base delay between retries (1ms-5min)
    Jitter = Jitter.Range(50, 200)  // Jitter configuration
};

// Or use builder pattern
var config = new RetryConfiguration()
    .WithRetryCount(5)
    .WithRetrySleep(1000)
    .WithJitter(Jitter.Percentage(25, 1000));
```

### Jitter Strategies

```csharp
// Range-based jitter (50-200ms)
Jitter.Range(50, 200)

// Percentage-based jitter (25% of base delay)
Jitter.Percentage(25, baseDelayMs: 1000)

// Simple max jitter (0-100ms)
Jitter.UpTo(100)

// No jitter
Jitter.None
```

## Predefined Policies

FluentRetry includes several predefined retry policies optimized for common scenarios:

```csharp
// Conservative - Fast operations (2 retries, 50ms delay)
Retry.With(action).WithConfiguration(RetryPolicies.Conservative).Run();

// Moderate - General purpose (3 retries, 150ms delay) - This is the default
Retry.With(action).WithConfiguration(RetryPolicies.Moderate).Run();

// Aggressive - Unreliable services (5 retries, 500ms delay)
Retry.With(action).WithConfiguration(RetryPolicies.Aggressive).Run();

// Network - HTTP calls (4 retries, 100ms delay with 25% jitter)
await httpClient.GetAsync(url).WithRetryAsync(RetryPolicies.Network);

// Database - Database operations (3 retries, 1000ms delay)
Retry.With(dbOperation).WithConfiguration(RetryPolicies.Database).Run();

// FileIO - File operations (5 retries, 25ms delay)
Retry.With(fileOperation).WithConfiguration(RetryPolicies.FileIO).Run();
```

### Custom Policies

```csharp
// Exponential backoff with custom parameters
var exponentialConfig = RetryPolicies.ExponentialBackoff(
    maxRetries: 4, 
    baseDelayMs: 100, 
    jitterPercentage: 20
);

// Linear backoff
var linearConfig = RetryPolicies.LinearBackoff(
    maxRetries: 3, 
    baseDelayMs: 100, 
    incrementMs: 100
);
```

## Advanced Features

### Exponential Backoff

```csharp
Retry.With(action)
    .UseExponentialRetry()  // Doubles delay on each retry
    .WithConfiguration(config)
    .Run();
```

### Exception Handling

Handle exceptions during retries and on final failure:

```csharp
Retry.With(action)
    .WithOnException(context => 
    {
        Console.WriteLine($"Attempt {context.AttemptNumber} failed: {context.ExceptionMessage}");
        Console.WriteLine($"Next retry in {context.RetrySleepInMs}ms");
    })
    .WithOnFinalException(context => 
    {
        Console.WriteLine($"All {context.TotalRetryCount + 1} attempts failed");
        LogError(context.Exception);
    })
    .ThrowOnFinalException(true)  // Throw exception after all retries exhausted
    .Run();
```

### Rich Retry Context

The `RetryContext` provides comprehensive information about each retry attempt:

```csharp
public class RetryContext
{
    public Exception Exception { get; }          // The exception that caused the retry
    public int RemainingRetry { get; }           // Remaining retry attempts
    public int RetrySleepInMs { get; }          // Delay before next retry
    public int AttemptNumber { get; }           // Current attempt number (1-based)
    public DateTimeOffset Timestamp { get; }    // When this context was created
    public int TotalRetryCount { get; }         // Total configured retry attempts
    public bool IsFinalAttempt { get; }         // Whether this is the final attempt
    public string ExceptionMessage { get; }     // Exception message or default
    public string ExceptionType { get; }        // Exception type name
}
```

### Jitter Control

```csharp
Retry.With(action)
    .SetJitterEnabled(false)  // Disable jitter completely
    .Run();
```

### Global Configuration

Set a global default configuration that applies to all retry operations:

```csharp
Retry.SetGlobalRetryConfiguration(new RetryConfiguration
{
    RetryCount = 5,
    RetrySleepInMs = 200,
    Jitter = Jitter.Percentage(20, 200)
});

// All subsequent retry operations will use this configuration by default
Retry.With(action).Run();
```

## Examples

### HTTP Client with Network Policy

```csharp
using var httpClient = new HttpClient();

// Retry HTTP calls with network-optimized settings
var response = await Retry.WithResultAsync(async () => 
    {
        var result = await httpClient.GetAsync("https://api.example.com/data");
        result.EnsureSuccessStatusCode();
        return result;
    })
    .WithConfiguration(RetryPolicies.Network)
    .WithOnException(context => 
        Console.WriteLine($"HTTP request failed, retrying in {context.RetrySleepInMs}ms..."))
    .ThrowOnFinalException(true)
    .Run();
```

### Database Operations with Exponential Backoff

```csharp
var connectionString = "...";

var data = await Retry.WithResultAsync(async () =>
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        // Your database operation here
        return await connection.QueryAsync<MyData>("SELECT * FROM MyTable");
    })
    .WithConfiguration(RetryPolicies.Database)
    .UseExponentialRetry()
    .WithOnException(context => 
        logger.LogWarning("Database operation failed on attempt {Attempt}: {Error}", 
            context.AttemptNumber, context.ExceptionMessage))
    .Run();
```

### File Operations with Custom Jitter

```csharp
var fileContent = Retry.WithResult(() => File.ReadAllText("important-file.txt"))
    .WithConfiguration(new RetryConfiguration
    {
        RetryCount = 10,
        RetrySleepInMs = 50,
        Jitter = Jitter.Range(10, 100)  // Random jitter between 10-100ms
    })
    .WithOnException(context => 
    {
        if (context.Exception is IOException)
            Console.WriteLine($"File locked, retrying in {context.RetrySleepInMs}ms...");
    })
    .Run();
```

## Best Practices

1. **Use Appropriate Policies**: Choose predefined policies that match your scenario (Network, Database, FileIO, etc.)

2. **Configure Jitter**: Always use jitter in distributed systems to avoid thundering herd problems

3. **Handle Exceptions Gracefully**: Use `WithOnException` for logging and monitoring

4. **Set Reasonable Limits**: Don't set retry counts too high; consider circuit breaker patterns for persistent failures

5. **Use Extension Methods**: They provide a cleaner, more readable syntax for simple scenarios

6. **Monitor Retry Behavior**: Use the rich context information for observability and debugging

## Performance

FluentRetry is optimized for high performance with:

- Minimal memory allocations
- Efficient exponential backoff calculation using bit shifting
- Overflow protection for extreme retry scenarios
- Static delegate caching where possible
- Optimized jitter generation using `Random.Shared`

## Thread Safety

FluentRetry is thread-safe. You can safely use the same configuration across multiple threads, and the global configuration can be set once during application startup.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.
