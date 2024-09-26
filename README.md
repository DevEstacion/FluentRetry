# FluentRetry

A lightweight, simple and fluent retry implementation for C# without any frills or fancy things.

## Installation

You can get the package from [nuget.org](https://www.nuget.org/packages/FluentRetry) or run the following from your
terminal.

```bash
dotnet add package FluentRetry
```

### The Api

Entrypoint class is called `Retry` which will route all fluency methods to the correct implementation. There are 4
entrypoint methods all prefixed with the word `With`.

### Prerequisites

Add the following using declaration in your class or in the `Using.cs`.

```csharp
using FluentRetry;
```

### API

```csharp
Retry.With(..)
Retry.WithAsync(..)
Retry.WithResult(..)
Retry.WithResultAsync(..)
Retry.SetGlobalRetryConfiguration(..)
```

### Handling Exceptions

You can handle the exceptions per retry and on the final exception separately. Use the following API below.

Each delegate will receive the `RetryContext` instance that contains the following.

```csharp
Exception Exception
int RemainingRetry
int RetrySleepInMs
```

#### On Each Exception

```csharp
.WithOnException(context =>
{
    // do something
});
```

#### On Final Exception

```csharp
.WithOnFinalException(context =>
{
    // do something
});
```

## Double Retry Sleep on Retry

You can enable a simple exponential backoff where the `RetrySleepInMs` is doubled on each retry. Default behavior is
disabled.

```csharp
.UseDoublingSleepOnRetry()
```

## Disable Jitter

An additional sleep ranging from 10 to 100ms is added as a "jitter" on each sleep retry. You can disable it by calling
the API below.

```csharp
.SetJitterEnabled()
```

## Custom Retry Configuration

Allows the caller to specify their own retry configurations on individual retry calls.

```csharp
.WithConfiguration(new RetryConfiguration
{
    RetryCount = 5,
    RetrySleepInMs = 1000,
    Jitter = Jitter.Range(10, 100)
})
```

## Setting the Global Retry Configuration

```csharp
Retry.SetGlobalRetryConfiguration(new RetryConfiguration
{
    RetryCount = 5,
    RetrySleepInMs = 1000,
    Jitter = Jitter.Range(10, 100)
})
```

### Default Values

There is an initial configuration set with the following values.

```csharp
RetryCount = 3,
RetrySleepInMs = 150,
Jitter = Jitter.Range(10, 100)
```
