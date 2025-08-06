namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void ThrowOnFinalException_DefaultBehavior_ShouldNotThrow()
    {
        // arrange
        var executionCount = 0;
        var finalExceptionCalled = false;
        var retry = new TestRetry(() =>
        {
            executionCount++;
            throw new InvalidOperationException("Test exception");
        });

        retry.WithOnFinalException(_ => finalExceptionCalled = true);

        // act
        retry.Execute().GetAwaiter().GetResult();

        // assert
        executionCount.Should().Be(4); // Initial + 3 retries (default configuration)
        finalExceptionCalled.Should().BeTrue("Final exception handler should be called");
    }

    [Fact]
    public void ThrowOnFinalException_EnabledTrue_ShouldThrow()
    {
        // arrange
        var executionCount = 0;
        var retry = new TestRetry(() =>
        {
            executionCount++;
            throw new InvalidOperationException("Test exception");
        });

        // act & assert
        Assert.Throws<InvalidOperationException>(() => retry.ThrowOnFinalException(true).Execute().GetAwaiter().GetResult());
        executionCount.Should().Be(4); // Initial + 3 retries (default configuration)
    }

    [Fact]
    public void ThrowOnFinalException_EnabledFalse_ShouldNotThrow()
    {
        // arrange
        var executionCount = 0;
        var finalExceptionCalled = false;
        var retry = new TestRetry(() =>
        {
            executionCount++;
            throw new InvalidOperationException("Test exception");
        });

        retry.WithOnFinalException(_ => finalExceptionCalled = true);

        // act
        retry.ThrowOnFinalException(false).Execute().GetAwaiter().GetResult();

        // assert
        executionCount.Should().Be(4); // Initial + 3 retries (default configuration)
        finalExceptionCalled.Should().BeTrue("Final exception handler should still be called");
    }

    [Fact]
    public void ThrowOnFinalException_EnabledFalse_WithCustomRetries_ShouldNotThrow()
    {
        // arrange
        var executionCount = 0;
        var finalExceptionCalled = false;
        var regularExceptionCount = 0;

        var retry = new TestRetry(() =>
        {
            executionCount++;
            throw new InvalidOperationException("Test exception");
        });

        retry.WithConfiguration(new RetryConfiguration { RetryCount = 2, RetrySleepInMs = 1 })
             .WithOnException(_ => regularExceptionCount++)
             .WithOnFinalException(_ => finalExceptionCalled = true);

        // act
        retry.ThrowOnFinalException(false).Execute().GetAwaiter().GetResult();

        // assert
        executionCount.Should().Be(3); // Initial + 2 retries
        regularExceptionCount.Should().Be(2); // Called for each retry (not final)
        finalExceptionCalled.Should().BeTrue("Final exception handler should still be called");
    }

    [Fact]
    public void ThrowOnFinalException_FluentInterface_ReturnsCorrectType()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        var result = retry.ThrowOnFinalException(true);

        // assert
        result.Should().BeSameAs(retry);
        retry.ShouldThrowOnFinalException.Should().BeTrue();
    }
}
