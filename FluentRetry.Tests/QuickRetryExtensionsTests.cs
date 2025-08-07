using FluentAssertions;
using Xunit;

namespace FluentRetry.Tests;

public class QuickRetryExtensionsTests
{
    [Fact]
    public void WithRetry_Action_ExecutesWithDefaultSettings()
    {
        // Arrange
        var invocations = 0;
        Action action = () =>
        {
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
        };

        // Act
        action.WithRetry();

        // Assert
        invocations.Should().Be(2);
    }

    [Fact]
    public void WithRetry_ActionWithAttempts_ExecutesWithSpecifiedAttempts()
    {
        // Arrange
        var invocations = 0;
        Action action = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        action.WithRetry(5);

        // Assert
        invocations.Should().Be(5);
    }

    [Fact]
    public void WithRetry_Function_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        Func<string> func = () =>
        {
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
            return $"Result {invocations}";
        };

        // Act
        var result = func.WithRetry();

        // Assert
        result.Should().Be("Result 2");
        invocations.Should().Be(2);
    }

    [Fact]
    public void WithRetry_FunctionWithAttempts_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        Func<int> func = () =>
        {
            invocations++;
            if (invocations < 4)
                throw new InvalidOperationException("Fails first three times");
            return invocations * 10;
        };

        // Act
        var result = func.WithRetry(5);

        // Assert
        result.Should().Be(40);
        invocations.Should().Be(4);
    }

    [Fact]
    public async Task WithRetryAsync_AsyncAction_ExecutesWithDefaultSettings()
    {
        // Arrange
        var invocations = 0;
        Func<Task> action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
        };

        // Act
        await action.WithRetryAsync();

        // Assert
        invocations.Should().Be(2);
    }

    [Fact]
    public async Task WithRetryAsync_AsyncActionWithAttempts_ExecutesWithSpecifiedAttempts()
    {
        // Arrange
        var invocations = 0;
        Func<Task> action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        await action.WithRetryAsync(4);

        // Assert
        invocations.Should().Be(4);
    }

    [Fact]
    public async Task WithRetryAsync_AsyncFunction_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        Func<Task<string>> func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
            return $"Result {invocations}";
        };

        // Act
        var result = await func.WithRetryAsync();

        // Assert
        result.Should().Be("Result 2");
        invocations.Should().Be(2);
    }

    [Fact]
    public async Task WithRetryAsync_AsyncFunctionWithAttempts_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        Func<Task<int>> func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            if (invocations < 3)
                throw new InvalidOperationException("Fails first two times");
            return invocations * 100;
        };

        // Act
        var result = await func.WithRetryAsync(5);

        // Assert
        result.Should().Be(300);
        invocations.Should().Be(3);
    }

    [Fact]
    public void WithExponentialRetry_Action_ExecutesWithExponentialBackoff()
    {
        // Arrange
        var invocations = 0;
        var delays = new List<long>();
        var lastTime = DateTimeOffset.UtcNow;

        Action action = () =>
        {
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
        action.WithExponentialRetry(attempts: 3, baseDelayMs: 50);

        // Assert
        invocations.Should().Be(3);
        delays.Should().HaveCount(2);
        delays[1].Should().BeGreaterThan(delays[0]); // Exponential backoff
    }

    [Fact]
    public async Task WithExponentialRetryAsync_AsyncAction_ExecutesWithExponentialBackoff()
    {
        // Arrange
        var invocations = 0;
        Func<Task> action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        await action.WithExponentialRetryAsync(attempts: 3, baseDelayMs: 10);

        // Assert
        invocations.Should().Be(3);
    }

    [Fact]
    public void WithExponentialRetry_Function_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        Func<string> func = () =>
        {
            invocations++;
            if (invocations < 3)
                throw new InvalidOperationException("Fails first two times");
            return $"Success {invocations}";
        };

        // Act
        var result = func.WithExponentialRetry(attempts: 5, baseDelayMs: 10);

        // Assert
        result.Should().Be("Success 3");
        invocations.Should().Be(3);
    }

    [Fact]
    public async Task WithExponentialRetryAsync_AsyncFunction_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        Func<Task<int>> func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
            return invocations * 50;
        };

        // Act
        var result = await func.WithExponentialRetryAsync(attempts: 4, baseDelayMs: 10);

        // Assert
        result.Should().Be(100);
        invocations.Should().Be(2);
    }

    [Fact]
    public void WithRetry_ActionThatNeverFails_ExecutesOnce()
    {
        // Arrange
        var invocations = 0;
        Action action = () => invocations++;

        // Act
        action.WithRetry(5);

        // Assert
        invocations.Should().Be(1);
    }

    [Fact]
    public void WithRetry_FunctionThatNeverFails_ExecutesOnce()
    {
        // Arrange
        var invocations = 0;
        Func<string> func = () =>
        {
            invocations++;
            return "Success";
        };

        // Act
        var result = func.WithRetry(5);

        // Assert
        result.Should().Be("Success");
        invocations.Should().Be(1);
    }

    [Fact]
    public async Task WithRetryAsync_AsyncActionThatNeverFails_ExecutesOnce()
    {
        // Arrange
        var invocations = 0;
        Func<Task> action = async () =>
        {
            await Task.Delay(1);
            invocations++;
        };

        // Act
        await action.WithRetryAsync(5);

        // Assert
        invocations.Should().Be(1);
    }

    [Fact]
    public async Task WithRetryAsync_AsyncFunctionThatNeverFails_ExecutesOnce()
    {
        // Arrange
        var invocations = 0;
        Func<Task<string>> func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            return "Success";
        };

        // Act
        var result = await func.WithRetryAsync(5);

        // Assert
        result.Should().Be("Success");
        invocations.Should().Be(1);
    }

    [Fact]
    public void WithRetry_ActionWithZeroAttempts_ExecutesOnce()
    {
        // Arrange
        var invocations = 0;
        Action action = () => invocations++;

        // Act
        action.WithRetry(0); // Should be clamped to 1

        // Assert
        invocations.Should().Be(1);
    }

    [Fact]
    public void WithExponentialRetry_WithDefaultParameters_UsesCorrectDefaults()
    {
        // Arrange
        var invocations = 0;
        Action action = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        action.WithExponentialRetry(); // Should use defaults: 4 attempts, 100ms delay

        // Assert
        invocations.Should().Be(4);
    }
}
