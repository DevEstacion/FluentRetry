namespace FluentRetry.Tests;

public class RetryBuilderTests
{
    [Fact]
    public void Execute_ActionSucceedsOnFirstAttempt_ExecutesOnce()
    {
        // Arrange
        var invocations = 0;
        int action() => invocations++;

        // Act
        Retry.Do(action).Execute();

        // Assert
        invocations.Should().Be(1);
    }

    [Fact]
    public void Execute_ActionFailsButSucceedsOnSecondAttempt_ExecutesTwice()
    {
        // Arrange
        var invocations = 0;
        void action()
        {
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
        }

        // Act
        Retry.Do(action)
            .Attempts(3)
            .Delay(1) // Minimal delay for fast tests
            .Execute();

        // Assert
        invocations.Should().Be(2);
    }

    [Fact]
    public void Execute_ActionFailsAllAttempts_DoesNotThrowByDefault()
    {
        // Arrange
        var invocations = 0;
        void action()
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        }

        // Act & Assert
        var act = () => Retry.Do(action)
            .Attempts(3)
            .Delay(1)
            .Execute();

        act.Should().NotThrow();
        invocations.Should().Be(3);
    }

    [Fact]
    public void Execute_ActionFailsAllAttemptsWithThrowOnFailure_ThrowsException()
    {
        // Arrange
        var invocations = 0;
        void action()
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        }

        // Act & Assert
        var act = () => Retry.Do(action)
            .Attempts(3)
            .Delay(1)
            .ThrowOnFailure()
            .Execute();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Always fails");
        invocations.Should().Be(3);
    }

    [Fact]
    public void Execute_WithCustomAttempts_RespectsMaxAttempts()
    {
        // Arrange
        var invocations = 0;
        void action()
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        }

        // Act
        Retry.Do(action)
            .Attempts(5)
            .Delay(1)
            .Execute();

        // Assert
        invocations.Should().Be(5);
    }

    [Fact]
    public void Execute_WithZeroDelay_ExecutesImmediately()
    {
        // Arrange
        var invocations = 0;
        void action()
        {
            invocations++;
            if (invocations < 3)
                throw new InvalidOperationException("Fails first two times");
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Retry.Do(action)
            .Attempts(3)
            .Delay(0)
            .Execute();
        stopwatch.Stop();

        // Assert
        invocations.Should().Be(3);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50); // Should be very fast
    }

    [Fact]
    public void Execute_WithDelay_WaitsBetweenRetries()
    {
        // Arrange
        var invocations = 0;
        void action()
        {
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Retry.Do(action)
            .Attempts(2)
            .Delay(100)
            .WithJitter(0) // No jitter for predictable timing
            .Execute();
        stopwatch.Stop();

        // Assert
        invocations.Should().Be(2);
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(90); // Should wait ~100ms
    }

    [Fact]
    public void Execute_WithExponentialBackoff_IncreasesDelayExponentially()
    {
        // Arrange
        var invocations = 0;
        var delays = new List<long>();
        var lastTime = DateTimeOffset.UtcNow;

        void action()
        {
            var now = DateTimeOffset.UtcNow;
            if (invocations > 0)
            {
                delays.Add((now - lastTime).Milliseconds);
            }
            lastTime = now;
            invocations++;
            throw new InvalidOperationException("Always fails");
        }

        // Act
        Retry.Do(action)
            .Attempts(3)
            .Delay(50)
            .WithExponentialBackoff()
            .WithJitter(0) // No jitter for predictable timing
            .Execute();

        // Assert
        invocations.Should().Be(3);
        delays.Should().HaveCount(2);
        delays[1].Should().BeGreaterThan(delays[0]); // Second delay should be longer
    }

    [Fact]
    public void Execute_OnRetryCallback_IsCalledOnEachRetry()
    {
        // Arrange
        var invocations = 0;
        var retryCallbacks = new List<(Exception ex, int attempt)>();

        void action()
        {
            invocations++;
            throw new InvalidOperationException($"Failure {invocations}");
        }

        // Act
        Retry.Do(action)
            .Attempts(3)
            .Delay(1)
            .OnRetry((ex, attempt) => retryCallbacks.Add((ex, attempt)))
            .Execute();

        // Assert
        invocations.Should().Be(3);
        retryCallbacks.Should().HaveCount(2); // Called on retries, not final attempt
        retryCallbacks[0].attempt.Should().Be(1);
        retryCallbacks[1].attempt.Should().Be(2);
        retryCallbacks[0].ex.Message.Should().Be("Failure 1");
        retryCallbacks[1].ex.Message.Should().Be("Failure 2");
    }

    [Fact]
    public void Execute_OnFailureCallback_IsCalledWhenAllRetriesFail()
    {
        // Arrange
        var invocations = 0;
        Exception? failureException = null;

        void action()
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        }

        // Act
        Retry.Do(action)
            .Attempts(2)
            .Delay(1)
            .OnFailure(ex => failureException = ex)
            .Execute();

        // Assert
        invocations.Should().Be(2);
        failureException.Should().NotBeNull();
        failureException!.Message.Should().Be("Always fails");
    }

    [Fact]
    public void Execute_OnFailureCallback_IsNotCalledWhenActionSucceeds()
    {
        // Arrange
        var invocations = 0;
        Exception? failureException = null;

        void action()
        {
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
        }

        // Act
        Retry.Do(action)
            .Attempts(2)
            .Delay(1)
            .OnFailure(ex => failureException = ex)
            .Execute();

        // Assert
        invocations.Should().Be(2);
        failureException.Should().BeNull();
    }

    [Fact]
    public void Execute_WithJitter_AddsRandomnessToDelay()
    {
        // Arrange
        var invocations = 0;
        void action()
        {
            invocations++;
            if (invocations < 3)
                throw new InvalidOperationException("Fails first two times");
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Retry.Do(action)
            .Attempts(3)
            .Delay(50)
            .WithJitter(100) // Add up to 100ms jitter
            .Execute();
        stopwatch.Stop();

        // Assert
        invocations.Should().Be(3);
        // With jitter, timing becomes less predictable, but it should still take some time
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(40);
    }

    [Fact]
    public void Execute_WithNegativeAttempts_UsesMinimumOfOne()
    {
        // Arrange
        var invocations = 0;
        int action() => invocations++;

        // Act
        Retry.Do(action)
            .Attempts(-5) // Should be clamped to 1
            .Execute();

        // Assert
        invocations.Should().Be(1);
    }

    [Fact]
    public void Execute_WithNegativeDelay_UsesZeroDelay()
    {
        // Arrange
        var invocations = 0;
        void action()
        {
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
        }

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Retry.Do(action)
            .Attempts(2)
            .Delay(-100) // Should be clamped to 0
            .Execute();
        stopwatch.Stop();

        // Assert
        invocations.Should().Be(2);
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(50); // Should be very fast
    }
}
