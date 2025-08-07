# FluentRetry

A lightweight, fluent retry helper for C#. Sensible defaults, minimal ceremony.

## Why FluentRetry?
- ✅ Simple: Two entry points `Retry.Do(...)` / `Retry.DoAsync(...)` cover all cases
- ⚙️ Configurable: Attempts, delay, exponential backoff, jitter, result- or exception-based retry
- 📦 Presets: `Fast()`, `Standard()`, `Resilient()`, `Network()`, `Database()` for one‑liners
- 🔁 Unified model: Same fluent builder for actions and functions (sync & async)
- 🧪 Tested: Comprehensive unit coverage

## Install
```bash
 dotnet add package FluentRetry
```

## Quick Examples
```csharp
using FluentRetry;

// Action with defaults (3 attempts, 150ms delay)
Retry.Do(() => CallService()).Execute();

// Function with return value
var data = Retry.Do(() => Fetch()).Execute();

// Async function
var dto = await Retry.DoAsync(async () => await client.GetAsync(url))
    .Network()       // preset: 4 attempts, base 100ms, exponential, jitter
    .ExecuteAsync();
```

## Fluent Configuration
```csharp
await Retry.DoAsync(async () => await UnstableCall())
    .Attempts(5)                // default: 3
    .Delay(200)                 // ms, default: 150
    .WithExponentialBackoff()   // doubles delay each retry (capped internally)
    .WithJitter(50)             // add 0..50ms random jitter (default: 50)
    .OnRetry((ex, attempt) => Console.WriteLine($"Attempt {attempt} failed: {ex.Message}"))
    .OnFailure(ex => Console.WriteLine($"All attempts failed: {ex.Message}"))
    .ThrowOnFailure()           // rethrow final failure (off by default)
    .ExecuteAsync();
```

### Retry Based on Result
```csharp
var result = Retry.Do(() => TryGetValue())
    .Attempts(5)
    .RetryWhen(v => v == null || v == 0)
    .Execute();
```

## Extension Shortcuts
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

## Presets
| Preset | Intent | Settings |
|--------|--------|----------|
| `Fast()` | Very quick operations | 2 attempts, 50ms, jitter 25ms |
| `Standard()` | Balanced default | 3 attempts, 150ms, jitter 50ms |
| `Resilient()` | Unstable deps | 5 attempts, 500ms, jitter 200ms |
| `Network()` | HTTP / flaky IO | 4 attempts, 100ms, exponential, jitter 50ms |
| `Database()` | DB timeouts | 3 attempts, 1000ms, jitter 200ms |

Usage:
```csharp
Retry.Do(() => Work()).Resilient().Execute();
var entity = await Retry.DoAsync(async () => await repo.Load())
    .Database()
    .ExecuteAsync();
```

## Global Defaults
```csharp
Retry.SetGlobalDefaults(attempts: 5, delayMs: 250);
// Subsequent builders pick up these defaults
Retry.Do(() => Work()).Execute();
```
(Existing builders keep the defaults captured at creation time.)

## API Surface
| Category | Methods |
|----------|---------|
| Entry | `Retry.Do(Action)`, `Retry.Do<T>(Func<T>)`, `Retry.DoAsync(Func<Task>)`, `Retry.DoAsync<T>(Func<Task<T>>)` |
| Core config | `.Attempts(int)`, `.Delay(int)`, `.WithExponentialBackoff()`, `.WithJitter(int)` |
| Behavior | `.ThrowOnFailure(bool=true)` |
| Callbacks | `.OnRetry(Action<Exception,int>)`, `.OnFailure(Action<Exception>)` |
| Result-based | `.RetryWhen(Func<T,bool>)` (generic only) |
| Presets | `.Fast()`, `.Standard()`, `.Resilient()`, `.Network()`, `.Database()` |
| Extensions | `action.WithRetry()`, `func.WithRetry()`, `action.WithExponentialRetry()`, `...Async` variants |
| Globals | `Retry.SetGlobalDefaults(int attempts, int delayMs)` |

## Typical Scenarios
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

## Error Semantics
- If all attempts fail and `ThrowOnFailure()` is NOT set, the last exception is suppressed (actions) or last result is returned (generic). For result retries that never satisfy the condition and `ThrowOnFailure()` is set, an `InvalidOperationException` is thrown.
- Cancellation (`OperationCanceledException`) is never retried and is rethrown immediately.

## Design Notes
- No external dependencies
- Thread-safe global defaults; individual builders are not intended for concurrent Execute calls
- Jitter adds 0..N ms uniformly to mitigate thundering herd
- Exponential backoff uses doubling with an internal safety cap

## Contributing
PRs welcome. Please include tests for behavioral changes.

## License
MIT © Contributors
