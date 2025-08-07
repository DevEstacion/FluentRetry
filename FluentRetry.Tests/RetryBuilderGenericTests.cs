using FluentAssertions;
using Xunit;

namespace FluentRetry.Tests;

public class RetryBuilderGenericTests
{
    [Fact]
    public void Execute_FunctionSucceedsOnFirstAttempt_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            return $"Result {invocations}";
        };

        // Act
        var result = Retry.Do(func).Execute();

        // Assert
        result.Should().Be("Result 1");
        invocations.Should().Be(1);
    }

    [Fact]
    public void Execute_FunctionFailsButSucceedsOnSecondAttempt_ReturnsCorrectValue()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
            return $"Result {invocations}";
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(3)
            .Delay(1)
            .Execute();

        // Assert
        result.Should().Be("Result 2");
        invocations.Should().Be(2);
    }

    [Fact]
    public void Execute_FunctionFailsAllAttempts_ReturnsDefault()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
            return "Should not reach here";
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(3)
            .Delay(1)
            .Execute();

        // Assert
        result.Should().BeNull();
        invocations.Should().Be(3);
    }

    [Fact]
    public void Execute_FunctionFailsAllAttemptsWithThrowOnFailure_ThrowsException()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
            return "Should not reach here";
        };

        // Act & Assert
        var act = () => Retry.Do(func)
            .Attempts(3)
            .Delay(1)
            .ThrowOnFailure()
            .Execute();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Always fails");
        invocations.Should().Be(3);
    }

    [Fact]
    public void Execute_WithRetryWhenCondition_RetriesBasedOnResult()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            return invocations < 3 ? 0 : 42; // Return 0 first two times, then 42
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(5)
            .Delay(1)
            .RetryWhen(value => value == 0) // Retry when result is 0
            .Execute();

        // Assert
        result.Should().Be(42);
        invocations.Should().Be(3);
    }

    [Fact]
    public void Execute_WithRetryWhenConditionNeverMet_ReturnsFirstResult()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            return 42; // Always returns non-zero
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(5)
            .Delay(1)
            .RetryWhen(value => value == 0) // Never true
            .Execute();

        // Assert
        result.Should().Be(42);
        invocations.Should().Be(1); // Should only execute once
    }

    [Fact]
    public void Execute_WithRetryWhenConditionAlwaysTrue_ExhaustsAllAttempts()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            return invocations;
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(3)
            .Delay(1)
            .RetryWhen(value => true) // Always retry
            .Execute();

        // Assert
        result.Should().Be(3); // Last attempt result
        invocations.Should().Be(3);
    }

    [Fact]
    public void Execute_WithRetryWhenConditionAlwaysTrueAndThrowOnFailure_ThrowsException()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            return invocations;
        };

        // Act & Assert
        var act = () => Retry.Do(func)
            .Attempts(3)
            .Delay(1)
            .RetryWhen(value => true) // Always retry
            .ThrowOnFailure()
            .Execute();

        act.Should().Throw<InvalidOperationException>()
            .WithMessage("Retry condition was not satisfied after all attempts");
        invocations.Should().Be(3);
    }

    [Fact]
    public void Execute_WithRetryWhenAndException_RetriesOnBothConditions()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
            if (invocations == 2)
                return 0; // Second attempt returns 0 (should retry)
            return 42; // Third attempt succeeds
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(5)
            .Delay(1)
            .RetryWhen(value => value == 0)
            .Execute();

        // Assert
        result.Should().Be(42);
        invocations.Should().Be(3);
    }

    [Fact]
    public void Execute_OnRetryCallbackWithResult_IsCalledForBothExceptionsAndRetryConditions()
    {
        // Arrange
        var invocations = 0;
        var retryCallbacks = new List<(Exception ex, int attempt)>();

        var func = () =>
        {
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
            if (invocations == 2)
                return 0; // Should trigger retry condition
            return 42;
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(5)
            .Delay(1)
            .RetryWhen(value => value == 0)
            .OnRetry((ex, attempt) => retryCallbacks.Add((ex, attempt)))
            .Execute();

        // Assert
        result.Should().Be(42);
        invocations.Should().Be(3);
        retryCallbacks.Should().HaveCount(2);
        retryCallbacks[0].ex.Should().BeOfType<InvalidOperationException>();
        retryCallbacks[1].ex.Should().BeOfType<InvalidOperationException>()
            .Which.Message.Should().Contain("Retry condition not met");
    }

    [Fact]
    public void Execute_ValueTypeFunction_HandlesValueTypesCorrectly()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
            return invocations * 10;
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(3)
            .Delay(1)
            .Execute();

        // Assert
        result.Should().Be(20);
        invocations.Should().Be(2);
    }

    [Fact]
    public void Execute_NullableValueTypeFunction_HandlesNullCorrectly()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            if (invocations == 1)
                return (int?)null;
            return 42;
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(3)
            .Delay(1)
            .RetryWhen(value => value == null)
            .Execute();

        // Assert
        result.Should().Be(42);
        invocations.Should().Be(2);
    }

    [Fact]
    public void Execute_ComplexObjectFunction_HandlesObjectsCorrectly()
    {
        // Arrange
        var invocations = 0;
        var func = () =>
        {
            invocations++;
            if (invocations == 1)
                throw new InvalidOperationException("First attempt fails");
            return new { Id = invocations, Name = $"Test {invocations}" };
        };

        // Act
        var result = Retry.Do(func)
            .Attempts(3)
            .Delay(1)
            .Execute();

        // Assert
        result.Should().NotBeNull();
        result!.Id.Should().Be(2);
        result.Name.Should().Be("Test 2");
        invocations.Should().Be(2);
    }
}
