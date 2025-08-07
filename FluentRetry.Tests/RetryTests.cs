namespace FluentRetry.Tests;

public class RetryTests
{
    [Fact]
    public void Do_WithAction_CreatesRetryBuilder()
    {
        // Arrange
        var invocations = 0;
        Action action = () => invocations++;

        // Act
        var builder = Retry.Do(action);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<RetryBuilder>();

        // Execute to verify it works
        builder.Execute();
        invocations.Should().Be(1);
    }

    [Fact]
    public void Do_WithNullAction_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => Retry.Do((Action)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DoAsync_WithAsyncAction_CreatesRetryBuilder()
    {
        // Arrange
        var invocations = 0;
        Func<Task> action = async () =>
        {
            await Task.Delay(1);
            invocations++;
        };

        // Act
        var builder = Retry.DoAsync(action);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<RetryBuilder>();

        // Execute to verify it works
        builder.Execute();
        invocations.Should().Be(1);
    }

    [Fact]
    public void DoAsync_WithNullAsyncAction_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => Retry.DoAsync((Func<Task>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Do_WithFunction_CreatesGenericRetryBuilder()
    {
        // Arrange
        var invocations = 0;
        Func<string> func = () =>
        {
            invocations++;
            return $"Result {invocations}";
        };

        // Act
        var builder = Retry.Do(func);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<RetryBuilder<string>>();

        // Execute to verify it works
        var result = builder.Execute();
        result.Should().Be("Result 1");
        invocations.Should().Be(1);
    }

    [Fact]
    public void Do_WithNullFunction_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => Retry.Do((Func<string>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void DoAsync_WithAsyncFunction_CreatesGenericRetryBuilder()
    {
        // Arrange
        var invocations = 0;
        Func<Task<int>> func = async () =>
        {
            await Task.Delay(1);
            invocations++;
            return invocations * 10;
        };

        // Act
        var builder = Retry.DoAsync(func);

        // Assert
        builder.Should().NotBeNull();
        builder.Should().BeOfType<RetryBuilder<int>>();

        // Execute to verify it works
        var result = builder.Execute();
        result.Should().Be(10);
        invocations.Should().Be(1);
    }

    [Fact]
    public void DoAsync_WithNullAsyncFunction_ThrowsArgumentNullException()
    {
        // Act & Assert
        var act = () => Retry.DoAsync((Func<Task<string>>)null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void SetGlobalDefaults_UpdatesDefaultValues()
    {
        // Arrange
        var originalDefaults = Retry.GetDefaults();

        try
        {
            // Act
            Retry.SetGlobalDefaults(attempts: 7, delayMs: 500);

            // Assert
            var newDefaults = Retry.GetDefaults();
            newDefaults.attempts.Should().Be(7);
            newDefaults.delayMs.Should().Be(500);

            // Verify new retry operations use the new defaults
            var invocations = 0;
            var action = () =>
            {
                invocations++;
                throw new InvalidOperationException("Always fails");
            };

            Retry.Do(action).Execute();
            invocations.Should().Be(7); // Should use new default
        }
        finally
        {
            // Cleanup - restore original defaults
            Retry.SetGlobalDefaults(originalDefaults.attempts, originalDefaults.delayMs);
        }
    }

    [Fact]
    public void SetGlobalDefaults_WithNegativeValues_ClampsToReasonableValues()
    {
        // Arrange
        var originalDefaults = Retry.GetDefaults();

        try
        {
            // Act
            Retry.SetGlobalDefaults(attempts: -5, delayMs: -100);

            // Assert
            var newDefaults = Retry.GetDefaults();
            newDefaults.attempts.Should().Be(1); // Should be clamped to minimum 1
            newDefaults.delayMs.Should().Be(0); // Should be clamped to minimum 0

            // But when used in practice, it should be clamped
            var invocations = 0;
            var action = () => invocations++;

            Retry.Do(action).Execute();
            invocations.Should().Be(1); // Should execute at least once regardless of negative attempts
        }
        finally
        {
            // Cleanup
            Retry.SetGlobalDefaults(originalDefaults.attempts, originalDefaults.delayMs);
        }
    }

    [Fact]
    public void GetDefaults_ReturnsCurrentDefaultValues()
    {
        // Act
        var defaults = Retry.GetDefaults();

        // Assert
        defaults.attempts.Should().BeGreaterThan(0);
        defaults.delayMs.Should().BeGreaterThanOrEqualTo(0);
    }

    [Fact]
    public void SetGlobalDefaults_AffectsSubsequentRetryOperations()
    {
        // Arrange
        var originalDefaults = Retry.GetDefaults();

        try
        {
            Retry.SetGlobalDefaults(attempts: 2, delayMs: 10);

            var invocations1 = 0;
            var invocations2 = 0;

            var action1 = () =>
            {
                invocations1++;
                throw new InvalidOperationException("Always fails");
            };

            var action2 = () =>
            {
                invocations2++;
                throw new InvalidOperationException("Always fails");
            };

            // Act
            Retry.Do(action1).Execute();
            Retry.Do(action2).Execute();

            // Assert
            invocations1.Should().Be(2);
            invocations2.Should().Be(2);
        }
        finally
        {
            // Cleanup
            Retry.SetGlobalDefaults(originalDefaults.attempts, originalDefaults.delayMs);
        }
    }

    [Fact]
    public void SetGlobalDefaults_DoesNotAffectExistingBuilders()
    {
        // Arrange
        var originalDefaults = Retry.GetDefaults();
        var invocations = 0;
        var action = () =>
        {
            invocations++;
            throw new InvalidOperationException("Always fails");
        };

        var builder = Retry.Do(action); // Create builder with current defaults

        try
        {
            // Act
            Retry.SetGlobalDefaults(attempts: 10, delayMs: 1000);

            // Execute the builder created before the global change
            builder.Execute();

            // Assert
            // Should use the defaults from when the builder was created
            invocations.Should().Be(originalDefaults.attempts);
        }
        finally
        {
            // Cleanup
            Retry.SetGlobalDefaults(originalDefaults.attempts, originalDefaults.delayMs);
        }
    }

    [Fact]
    public void Retry_StaticClass_CannotBeInstantiated()
    {
        // Assert - static classes in C# are both abstract and sealed
        var type = typeof(Retry);
        type.IsAbstract.Should().BeTrue();
        type.IsSealed.Should().BeTrue();
        type.IsClass.Should().BeTrue();

        // Ensure no public constructors
        type.GetConstructors().Should().BeEmpty();
    }

    [Theory]
    [InlineData(1, 50)]
    [InlineData(5, 200)]
    [InlineData(10, 1000)]
    public void SetGlobalDefaults_WithVariousValues_StoresCorrectly(int attempts, int delayMs)
    {
        // Arrange
        var originalDefaults = Retry.GetDefaults();

        try
        {
            // Act
            Retry.SetGlobalDefaults(attempts, delayMs);

            // Assert
            var newDefaults = Retry.GetDefaults();
            newDefaults.attempts.Should().Be(attempts);
            newDefaults.delayMs.Should().Be(delayMs);
        }
        finally
        {
            // Cleanup
            Retry.SetGlobalDefaults(originalDefaults.attempts, originalDefaults.delayMs);
        }
    }
}
