# FluentRetry

Lightweight, fluent retries for C#. Sensible defaults. Minimal ceremony. Maximum clarity.

## TL;DR

- Install
    ```bash
    dotnet add package FluentRetry
    ```
- Use
    ```csharp
    using FluentRetry;

    // Sync action (defaults: 3 attempts, 150ms delay + jitter)
    Retry.Do(() => CallService()).Execute();

    // Async with preset
    var dto = await Retry.DoAsync(async () => await client.GetAsync(url))
            .Network()     // 4 attempts, base 100ms, exponential, jitter
            .ExecuteAsync();
    ```

## Why FluentRetry?
- ✅ Simple: Two entry points cover sync & async — `Retry.Do(...)` / `Retry.DoAsync(...)`
- ⚙️ Flexible: Attempts, delay, exponential backoff, jitter, result- or exception-based retry
- 📦 One‑liners: `Fast()`, `Standard()`, `Resilient()`, `Network()`, `Database()` presets
- 🔁 Unified builder: Same fluent model for actions and functions (sync & async)
- 🧪 Solid: Comprehensive unit tests

## Quick start

```csharp
using FluentRetry;

// Action with defaults
Retry.Do(() => CallService()).Execute();

// Function with return value
var data = Retry.Do(() => Fetch()).Execute();

// Async function with network preset
var dto = await Retry.DoAsync(async () => await client.GetAsync(url))
        .Network()
        .ExecuteAsync();
```

## Common recipes

### HTTP (with backoff)
```csharp
var body = await Retry.DoAsync(async () => {
                var resp = await client.GetAsync(url);
                resp.EnsureSuccessStatusCode();
                return await resp.Content.ReadAsStringAsync();
        })
        .Network()
        .ThrowOnFailure()
        .ExecuteAsync();
```

### Database
```csharp
var rows = await Retry.DoAsync(async () => await db.QueryAsync<User>(sql))
        .Database()
        .ExecuteAsync();
```

### File IO (transient locks)
```csharp
var text = Retry.Do(() => File.ReadAllText(path))
        .Fast()
        .Execute();
```

## Fluent configuration

```csharp
await Retry.DoAsync(async () => await UnstableCall())
        .Attempts(5)                // default: 3
        .Delay(200)                 // ms, default: 150
        .WithExponentialBackoff()   // doubles delay each retry (safety cap inside)
        .WithJitter(50)             // add 0..50ms random jitter (default: 50)
        .OnRetry((ex, attempt) => Console.WriteLine($"Attempt {attempt} failed: {ex.Message}"))
        .OnFailure(ex => Console.WriteLine($"All attempts failed: {ex.Message}"))
        .ThrowOnFailure()           // rethrow final failure (off by default)
        .ExecuteAsync();
```

### Retry based on result
```csharp
var result = Retry.Do(() => TryGetValue())
        .Attempts(5)
        .RetryWhen(v => v == null || v == 0)
        .Execute();
```

## Presets

| Preset       | Intent             | Settings                                   |
|--------------|--------------------|--------------------------------------------|
| `Fast()`     | Very quick work    | 2 attempts, 50ms, jitter 25ms              |
| `Standard()` | Balanced default   | 3 attempts, 150ms, jitter 50ms             |
| `Resilient()`| Unstable deps      | 5 attempts, 500ms, jitter 200ms            |
| `Network()`  | HTTP/flaky IO      | 4 attempts, 100ms, exponential, jitter 50ms|
| `Database()` | DB timeouts        | 3 attempts, 300ms, jitter 100ms            |

Usage:
```csharp
Retry.Do(() => Work()).Resilient().Execute();
var entity = await Retry.DoAsync(async () => await repo.Load())
        .Database()
        .ExecuteAsync();
```

## Extension shortcuts

```csharp
// Action
action.WithRetry();
action.WithRetry(5);

// Function
var value = func.WithRetry();
var other = await asyncFunc.WithRetryAsync();

// Exponential
action.WithExponentialRetry();
var payload = await asyncFunc.WithExponentialRetryAsync(attempts: 4, baseDelayMs: 100);
```

## Global defaults

```csharp
Retry.SetGlobalDefaults(attempts: 5, delayMs: 250);
// Subsequent builders pick up these defaults
Retry.Do(() => Work()).Execute();
```

Note: Existing builders keep the defaults captured at their creation time.

## API at a glance

| Category     | Methods                                                                                 |
|--------------|-----------------------------------------------------------------------------------------|
| Entry        | `Retry.Do(Action)`, `Retry.Do<T>(Func<T>)`, `Retry.DoAsync(Func<Task>)`, `Retry.DoAsync<T>(Func<Task<T>>)` |
| Core config  | `.Attempts(int)`, `.Delay(int)`, `.WithExponentialBackoff()`, `.WithJitter(int)`        |
| Behavior     | `.ThrowOnFailure(bool = true)`                                                          |
| Callbacks    | `.OnRetry(Action<Exception,int>)`, `.OnFailure(Action<Exception>)`                      |
| Result-based | `.RetryWhen(Func<T,bool>)` (generic only)                                               |
| Presets      | `.Fast()`, `.Standard()`, `.Resilient()`, `.Network()`, `.Database()`                   |
| Extensions   | `action.WithRetry()`, `func.WithRetry()`, `action.WithExponentialRetry()`, `...Async` variants |
| Globals      | `Retry.SetGlobalDefaults(int attempts, int delayMs)`                                    |

## Error semantics

- If all attempts fail and `ThrowOnFailure()` is NOT set:
    - Actions suppress the last exception.
    - Generic (result) returns the last produced result.
- If result retries never satisfy the condition and `ThrowOnFailure()` is set, an `InvalidOperationException` is thrown.
- `OperationCanceledException` is never retried and is rethrown immediately.

## Design notes

- No external dependencies
- Thread-safe global defaults; individual builders are not intended for concurrent Execute calls
- Jitter adds 0..N ms uniformly to mitigate thundering herd
- Exponential backoff uses doubling with an internal safety cap

## FAQ

- When should I enable exponential backoff?
    - For networked calls or rate-limited services where spacing retries reduces pressure.
- Why jitter?
    - To avoid synchronized retries (“thundering herd”) when many clients fail at once.
- Do exceptions or results control retries?
    - Both are supported: retry on exception by default, or use `.RetryWhen(...)` for result-based control.

## Contributing

PRs welcome. Please include tests for behavioral changes.

## License

MIT © Contributors
