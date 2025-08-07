using FluentAssertions;
using Xunit;

namespace FluentRetry.Tests;

public class RetryPresetsTests
{
    [Fact]
    public void Fast_ConfiguresCorrectSettings()
    {
        // Arrange
        var invocations = 0;
        var action = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        Retry.Do(action)
            .Fast()
            .Execute();

        // Assert
        invocations.Should().Be(2); // Fast preset should use 2 attempts
    }

    [Fact]
    public void Standard_ConfiguresCorrectSettings()
    {
        // Arrange
        var invocations = 0;
        var action = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        Retry.Do(action)
            .Standard()
            .Execute();

        // Assert
        invocations.Should().Be(3); // Standard preset should use 3 attempts
    }

    [Fact]
    public void Resilient_ConfiguresCorrectSettings()
    {
        // Arrange
        var invocations = 0;
        var action = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        Retry.Do(action)
            .Resilient()
            .Execute();

        // Assert
        invocations.Should().Be(5); // Resilient preset should use 5 attempts
    }

    [Fact]
    public void Network_ConfiguresExponentialBackoff()
    {
        // Arrange
        var invocations = 0;
        var delays = new List<long>();
        var lastTime = DateTimeOffset.UtcNow;

        var action = () =>
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
        Retry.Do(action)
            .Network()
            .Execute();

        // Assert
        invocations.Should().Be(4); // Network preset should use 4 attempts
        delays.Should().HaveCount(3);
        // Should have exponential backoff (second delay > first delay)
        delays[1].Should().BeGreaterThan(delays[0]);
    }

    [Fact]
    public void Database_ConfiguresLongerDelays()
    {
        // Arrange
        var invocations = 0;
        var action = () =>
        {
            invocations++;
            if (invocations < 2)
                throw new InvalidOperationException("Fails first time");
        };

        // Act
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();
        Retry.Do(action)
            .Database()
            .Execute();
        stopwatch.Stop();

        // Assert
        invocations.Should().Be(2);
        // Database preset should have longer delays (base 1000ms, but with jitter it varies)
        stopwatch.ElapsedMilliseconds.Should().BeGreaterThan(500);
    }

    [Fact]
    public void Fast_Generic_ConfiguresCorrectSettings()
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
            .Fast()
            .Execute();

        // Assert
        invocations.Should().Be(2);
        result.Should().BeNull();
    }

    [Fact]
    public void Resilient_Generic_ConfiguresCorrectSettings()
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
            .Resilient()
            .Execute();

        // Assert
        invocations.Should().Be(5);
        result.Should().BeNull();
    }

    [Fact]
    public void Network_Generic_ConfiguresExponentialBackoff()
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
            .Network()
            .Execute();

        // Assert
        invocations.Should().Be(4);
        result.Should().BeNull();
    }

    [Fact]
    public void PresetsCanBeOverridden_AdditionalConfigurationApplied()
    {
        // Arrange
        var invocations = 0;
        var action = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        Retry.Do(action)
            .Fast() // 2 attempts
            .Attempts(7) // Override to 7 attempts
            .Execute();

        // Assert
        invocations.Should().Be(7); // Should use the overridden value
    }

    [Fact]
    public void PresetsCanBeChained_LastConfigurationWins()
    {
        // Arrange
        var invocations = 0;
        var action = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        Retry.Do(action)
            .Fast() // 2 attempts
            .Resilient() // 5 attempts - should override
            .Execute();

        // Assert
        invocations.Should().Be(5); // Should use Resilient settings
    }

    [Fact]
    public async Task Fast_Async_ConfiguresCorrectSettings()
    {
        // Arrange
        var invocations = 0;
        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        await Retry.DoAsync(action)
            .Fast()
            .ExecuteAsync();

        // Assert
        invocations.Should().Be(2);
    }

    [Fact]
    public async Task Network_Async_ConfiguresExponentialBackoff()
    {
        // Arrange
        var invocations = 0;
        var action = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        await Retry.DoAsync(action)
            .Network()
            .ExecuteAsync();

        // Assert
        invocations.Should().Be(4);
    }

    [Fact]
    public async Task Database_AsyncGeneric_ConfiguresCorrectSettings()
    {
        // Arrange
        var invocations = 0;
        var func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            throw new InvalidOperationException("Always fails");
            return "Should not reach here";
        };

        // Act
        var result = await Retry.DoAsync(func)
            .Database()
            .ExecuteAsync();

        // Assert
        invocations.Should().Be(3); // Database preset uses 3 attempts
        result.Should().BeNull();
    }
}
