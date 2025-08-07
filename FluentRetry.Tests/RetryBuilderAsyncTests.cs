namespace FluentRetry.Tests;

public class RetryBuilderAsyncTests
{
    [Fact]
    public async Task ExecuteAsync_AsyncActionSucceedsOnFirstAttempt_ExecutesOnce()
    {
        // Arrange
        var invocations = 0;
        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
        };

        // Act
        await Retry.DoAsync(action).ExecuteAsync();

        // Assert
        invocations.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_AsyncActionFailsButSucceedsOnSecondAttempt_ExecutesTwice()
    {
        // Arrange
        var invocations = 0;
        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
        };

        // Act
        await Retry.DoAsync(action)
            .Attempts(3)
            .Delay(1)
            .ExecuteAsync();

        // Assert
        invocations.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_AsyncActionFailsAllAttempts_DoesNotThrowByDefault()
    {
        // Arrange
        var invocations = 0;
        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act & Assert
        var act = async () => await Retry.DoAsync(action)
            .Attempts(3)
            .Delay(1)
            .ExecuteAsync();

        await act.Should().NotThrowAsync();
        invocations.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_AsyncActionFailsAllAttemptsWithThrowOnFailure_ThrowsException()
    {
        // Arrange
        var invocations = 0;
        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act & Assert
        var act = async () => await Retry.DoAsync(action)
            .Attempts(3)
            .Delay(1)
            .ThrowOnFailure()
            .ExecuteAsync();

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("Always fails");
        invocations.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_AsyncFunctionSucceedsOnFirstAttempt_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        var func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            return $"Result {invocations}";
        };

        // Act
        var result = await Retry.DoAsync(func).ExecuteAsync();

        // Assert
        result.Should().Be("Result 1");
        invocations.Should().Be(1);
    }

    [Fact]
    public async Task ExecuteAsync_AsyncFunctionFailsButSucceedsOnSecondAttempt_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        var func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
            return $"Result {invocations}";
        };

        // Act
        var result = await Retry.DoAsync(func)
            .Attempts(3)
            .Delay(1)
            .ExecuteAsync();

        // Assert
        result.Should().Be("Result 2");
        invocations.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_AsyncFunctionWithRetryWhen_RetriesBasedOnResult()
    {
        // Arrange
        var invocations = 0;
        var func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            return invocations < 3 ? 0 : 42;
        };

        // Act
        var result = await Retry.DoAsync(func)
            .Attempts(5)
            .Delay(1)
            .RetryWhen(value => value == 0)
            .ExecuteAsync();

        // Assert
        result.Should().Be(42);
        invocations.Should().Be(3);
    }

    [Fact]
    public async Task ExecuteAsync_WithAsyncDelay_WaitsBetweenRetries()
    {
        // Arrange
        var invocations = 0;
        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        await Retry.DoAsync(action)
            .Attempts(2)
            .Delay(100)
            .WithJitter(0)
            .ExecuteAsync();
        stopwatch.Stop();

        // Assert
        invocations.Should().Be(2);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(90);
    }

    [Fact]
    public async Task ExecuteAsync_OnRetryCallback_IsCalledOnEachRetry()
    {
        // Arrange
        var invocations = 0;
        var retryCallbacks = new List<(Exception ex, int attempt)>();

        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException($"Failure {invocations}");
        };

        // Act
        await Retry.DoAsync(action)
            .Attempts(3)
            .Delay(1)
            .OnRetry((ex, attempt) => retryCallbacks.Add((ex, attempt)))
            .ExecuteAsync();

        // Assert
        invocations.Should().Be(3);
        retryCallbacks.Should().HaveCount(2);
        retryCallbacks[0].attempt.Should().Be(1);
        retryCallbacks[1].attempt.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_OnFailureCallback_IsCalledWhenAllRetriesFail()
    {
        // Arrange
        var invocations = 0;
        Exception? failureException = null;

        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        await Retry.DoAsync(action)
            .Attempts(2)
            .Delay(1)
            .OnFailure(ex => failureException = ex)
            .ExecuteAsync();

        // Assert
        invocations.Should().Be(2);
        failureException.Should().NotBeNull();
        failureException!.Message.Should().Be("Always fails");
    }

    [Fact]
    public async Task ExecuteAsync_CancellationDuringOperation_PropagatesCancellation()
    {
        // Arrange
        using var cts = new CancellationTokenSource();
        var invocations = 0;

        var func = async () =>
        {
            invocations++;
            await Task.Delay(100, cts.Token); // Will be cancelled
            return "Should not reach here";
        };

        // Act & Assert
        cts.CancelAfter(50); // Cancel after 50ms

        var act = async () => await Retry.DoAsync(func)
            .Attempts(3)
            .Delay(1)
            .ExecuteAsync();

        await act.Should().ThrowAsync<OperationCanceledException>();
        invocations.Should().Be(1); // Should not retry on cancellation
    }

    [Fact]
    public async Task ExecuteAsync_TaskThatReturnsFaulted_HandlesExceptionCorrectly()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            if (invocations == 1)
                return Task.FromException<string>(new InvalidOperationException("Task failed"));
            return Task.FromResult($"Success {invocations}");
        };

        // Act
        var result = await Retry.DoAsync(func)
            .Attempts(3)
            .Delay(1)
            .ExecuteAsync();

        // Assert
        result.Should().Be("Success 2");
        invocations.Should().Be(2);
    }

    [Fact]
    public async Task ExecuteAsync_TaskThatReturnsCancelled_PropagatesCancellation()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            return Task.FromCanceled<string>(new CancellationToken(true));
        };

        // Act & Assert
        var act = async () => await Retry.DoAsync(func)
            .Attempts(3)
            .Delay(1)
            .ExecuteAsync();

        await act.Should().ThrowAsync<OperationCanceledException>();
        invocations.Should().Be(1); // Should not retry on cancellation
    }

    [Fact]
    public async Task ExecuteAsync_WithExponentialBackoffAsync_IncreasesDelayExponentially()
    {
        // Arrange
        var invocations = 0;
        var delays = new List<long>();
        var lastTime = DateTimeOffset.UtcNow;

        var action = async () =>
        {
            await Task.Delay(1);
            var now = DateTimeOffset.UtcNow;
            if (invocations > 0)
            {
                delays.Add((now - lastTime).Milliseconds);
            }
            lastTime = now;
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        await Retry.DoAsync(action)
            .Attempts(3)
            .Delay(50)
            .WithExponentialBackoff()
            .WithJitter(0)
            .ExecuteAsync();

        // Assert
        invocations.Should().Be(3);
        delays.Should().HaveCount(2);
        delays[1].Should().BeGreaterThan(delays[0]);
    }

    [Fact]
    public async Task ExecuteAsync_ConcurrentExecution_HandlesMultipleCallsCorrectly()
    {
        // Arrange
        var invocations = 0;
        var func = async () =>
        {
            await Task.Delay(10);
            Interlocked.Increment(ref invocations);
            if (invocations <= 2)
                throw new InvalidOperationException("First two fail");
            return invocations;
        };

        // Act
        var tasks = Enumerable.Range(0, 3).Select(_ =>
            Retry.DoAsync(func)
                .Attempts(5)
                .Delay(1)
                .ExecuteAsync()
        ).ToArray();

        var results = await Task.WhenAll(tasks);

        // Assert
        results.Should().AllSatisfy(r => r.Should().BeGreaterThan(0));
        invocations.Should().BeGreaterThan(2);
    }
}
