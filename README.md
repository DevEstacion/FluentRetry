# FluentRetry

A lightweight, simple and fluent retry implementation for C# that just works. Built for simplicity and ease of use with sensible defaults.

## Features

✨ **Simple & Fluent API** - Easy-to-use fluent interface for all retry scenarios
🚀 **Works Out of the Box** - Sensible defaults, no configuration required
🎯 **Unified Methods** - Single `Do()` and `DoAsync()` methods handle all scenarios
📋 **Predefined Configurations** - Ready-to-use settings for common scenarios
🔧 **Flexible** - Extensive configuration options when needed
⚡ **High Performance** - Optimized retry logic with minimal allocations

## Installation

```bash
dotnet add package FluentRetry
```

## Quick Start

```csharp
using FluentRetry;

// Simple action with default settings (3 attempts, 150ms delay)
Retry.Do(() => Console.WriteLine("Hello")).Execute();

// Function with return value
var result = Retry.Do(() => GetDataFromAPI()).Execute();

// Async operations
await Retry.DoAsync(async () => await httpClient.GetAsync(url)).ExecuteAsync();
```

## Basic Usage

### Actions (No Return Value)

```csharp
// Simple action retry
Retry.Do(() => Console.WriteLine("Hello")).Execute();

// Action with configuration
Retry.Do(() => RiskyOperation())
    .Attempts(5)
    .Delay(200)
    .WithExponentialBackoff()
    .Execute();

// Async action
await Retry.DoAsync(async () => await httpClient.GetAsync(url))
    .Network() // Predefined network configuration
    .ExecuteAsync();
```

### Functions (With Return Value)

```csharp
// Function with return value
var result = Retry.Do(() => GetDataFromAPI())
    .Attempts(3)
    .OnRetry((ex, attempt) => Console.WriteLine($"Attempt {attempt} failed: {ex.Message}"))
    .Execute();

// Async function with return value
var data = await Retry.DoAsync(async () => await database.QueryAsync())
    .Resilient() // Uses predefined resilient configuration
    .RetryWhen(result => result == null)
    .ExecuteAsync();
```

## One-Line Extension Methods

For simple cases, use extension methods for the most concise syntax:

```csharp
// Simple extension method usage
action.WithRetry();
action.WithRetry(5); // 5 attempts

// Async extensions
await asyncAction.WithRetryAsync();
await asyncAction.WithRetryAsync(3);

// With exponential backoff
action.WithExponentialRetry();
action.WithExponentialRetry(attempts: 5, baseDelayMs: 200);

// Functions
var result = func.WithRetry();
var data = await asyncFunc.WithRetryAsync();
```

## Configuration Options

### Basic Configuration

```csharp
Retry.Do(action)
    .Attempts(5)                    // Number of attempts (default: 3)
    .Delay(100)                     // Delay between retries in ms (default: 150ms)
    .WithExponentialBackoff()       // Doubles delay on each retry
    .WithJitter(50)                 // Add randomness to delays (default: 50ms)
    .ThrowOnFailure()               // Throw exception if all retries fail
    .Execute();
```

### Exception Handling

```csharp
Retry.Do(action)
    .OnRetry((ex, attempt) =>
        Console.WriteLine($"Attempt {attempt} failed: {ex.Message}"))
    .OnFailure(ex =>
        Console.WriteLine($"All attempts failed: {ex.Message}"))
    .Execute();
```

### Retry Conditions (For Functions)

```csharp
// Retry based on return value
var result = Retry.Do(() => TryGetValue())
    .Attempts(3)
    .RetryWhen(value => value == null || value == 0)
    .Execute();
```

## Predefined Configurations

Use predefined configurations for common scenarios:

```csharp
// Fast operations - minimal delays (2 attempts, 50ms delay)
Retry.Do(action).Fast().Execute();

// Standard operations - balanced settings (3 attempts, 150ms delay) - DEFAULT
Retry.Do(action).Standard().Execute();

// Resilient operations - more retries for unreliable services (5 attempts, 500ms delay)
Retry.Do(action).Resilient().Execute();

// Network operations - exponential backoff for HTTP calls (4 attempts, 100ms delay)
Retry.Do(action).Network().Execute();

// Database operations - longer delays for database timeouts (3 attempts, 1000ms delay)
Retry.Do(action).Database().Execute();
```

## Advanced Examples

### HTTP Client with Retry

```csharp
using var httpClient = new HttpClient();

// Simple HTTP retry
var response = await Retry.DoAsync(async () =>
        await httpClient.GetAsync("https://api.example.com/data"))
    .Network()
    .ExecuteAsync();

// With custom error handling
var result = await Retry.DoAsync(async () =>
    {
        var response = await httpClient.GetAsync(url);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadAsStringAsync();
    })
    .Attempts(5)
    .Delay(100)
    .WithExponentialBackoff()
    .OnRetry((ex, attempt) =>
        Console.WriteLine($"HTTP request failed on attempt {attempt}: {ex.Message}"))
    .ThrowOnFailure()
    .ExecuteAsync();
```

### Database Operations

```csharp
// Database query with retry
var users = await Retry.DoAsync(async () =>
    {
        using var connection = new SqlConnection(connectionString);
        await connection.OpenAsync();
        return await connection.QueryAsync<User>("SELECT * FROM Users");
    })
    .Database()
    .OnRetry((ex, attempt) =>
        logger.LogWarning($"Database query failed on attempt {attempt}: {ex.Message}"))
    .ExecuteAsync();
```

### File Operations

```csharp
// File read with retry
var content = Retry.Do(() => File.ReadAllText(filePath))
    .Fast()
    .OnRetry((ex, attempt) =>
        Console.WriteLine($"File read failed on attempt {attempt}, retrying..."))
    .Execute();
```

### Complex Retry Logic

```csharp
// Full configuration example
await Retry.DoAsync(async () => await SomeAsyncOperation())
    .Attempts(5)
    .Delay(100)
    .WithExponentialBackoff()
    .WithJitter(50)
    .OnRetry((ex, attempt) =>
        logger.LogWarning($"Attempt {attempt} failed: {ex.Message}"))
    .OnFailure(ex =>
        logger.LogError($"All attempts failed: {ex.Message}"))
    .ThrowOnFailure()
    .ExecuteAsync();

// Retry based on result condition
var result = Retry.Do(() => TryGetValue())
    .Attempts(3)
    .Delay(100)
    .RetryWhen(value => value == null || value == 0)
    .OnRetry((ex, attempt) =>
        Console.WriteLine($"Got invalid result on attempt {attempt}, retrying..."))
    .Execute();
```

## Global Configuration

Set global defaults for all retry operations:

```csharp
// Set global defaults
Retry.SetGlobalDefaults(attempts: 5, delayMs: 200);

// All subsequent operations will use these defaults
Retry.Do(action).Execute(); // Uses 5 attempts, 200ms delay
```

## API Reference

### Core Methods

| Method | Description |
|--------|-------------|
| `Retry.Do(Action)` | Creates retry for action without return value |
| `Retry.DoAsync(Func<Task>)` | Creates retry for async action without return value |
| `Retry.Do<T>(Func<T>)` | Creates retry for function with return value |
| `Retry.DoAsync<T>(Func<Task<T>>)` | Creates retry for async function with return value |

### Configuration Methods

| Method | Description |
|--------|-------------|
| `.Attempts(int)` | Sets maximum number of attempts (default: 3) |
| `.Delay(int)` | Sets delay between retries in milliseconds (default: 150ms) |
| `.WithExponentialBackoff()` | Enables exponential backoff (doubles delay each retry) |
| `.WithJitter(int)` | Sets maximum jitter in milliseconds (default: 50ms) |
| `.ThrowOnFailure(bool)` | Whether to throw exception on final failure (default: false) |
| `.OnRetry(Action<Exception, int>)` | Callback for each retry attempt |
| `.OnFailure(Action<Exception>)` | Callback when all retries are exhausted |
| `.RetryWhen(Func<T, bool>)` | Condition to retry based on result (functions only) |

### Predefined Configurations

| Method | Configuration |
|--------|---------------|
| `.Fast()` | 2 attempts, 50ms delay, 25ms jitter |
| `.Standard()` | 3 attempts, 150ms delay, 50ms jitter (default) |
| `.Resilient()` | 5 attempts, 500ms delay, 200ms jitter |
| `.Network()` | 4 attempts, 100ms delay, exponential backoff, 50ms jitter |
| `.Database()` | 3 attempts, 1000ms delay, 200ms jitter |

### Extension Methods

| Method | Description |
|--------|-------------|
| `action.WithRetry()` | Execute action with default retry |
| `action.WithRetry(int)` | Execute action with specified attempts |
| `action.WithExponentialRetry()` | Execute action with exponential backoff |
| `func.WithRetry()` | Execute function with default retry |
| `asyncAction.WithRetryAsync()` | Execute async action with default retry |
| `asyncFunc.WithRetryAsync()` | Execute async function with default retry |

## Migration from Old API

If you're upgrading from an older version:

| Old API | New API |
|---------|---------|
| `Retry.With(action)` | `Retry.Do(action)` |
| `Retry.WithAsync(action)` | `Retry.DoAsync(action)` |
| `Retry.WithResult<T>(func)` | `Retry.Do<T>(func)` |
| `Retry.WithResultAsync<T>(func)` | `Retry.DoAsync<T>(func)` |
| `.MaxAttempts(n)` | `.Attempts(n)` |
| `.WithConfiguration(config)` | Use predefined configurations or individual methods |
| `.UseExponentialRetry()` | `.WithExponentialBackoff()` |
| `.ThrowOnFinalException(true)` | `.ThrowOnFailure(true)` |
| `.RetryIf(condition)` | `.RetryWhen(condition)` |

## Key Improvements

1. **Unified API**: Single `Retry.Do()` and `Retry.DoAsync()` methods handle all scenarios
2. **Simple Configuration**: Fluent builder with intuitive method names
3. **Predefined Configurations**: Ready-to-use settings for common scenarios
4. **Extension Methods**: One-line retry for simple cases
5. **Works Out of the Box**: Sensible defaults, no configuration required
6. **Cleaner**: Fewer classes and methods to understand
7. **Type-Safe**: Proper generic support for return values
8. **Performance**: Optimized with minimal allocations

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Contributing

Contributions are welcome! Please feel free to submit a Pull Request.
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
