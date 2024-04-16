using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void WithSkipExceptions_DuplicateException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        retry.WithSkipExceptions(typeof(TestCanceledException), typeof(TestCanceledException));

        // assert
        retry.ExceptionsToSkip.Should().NotBeEmpty();
        retry.ExceptionsToSkip.Should().OnlyContain(x => x == typeof(TestCanceledException));
    }

    [Fact]
    public void WithSkipExceptions_EmptyException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        retry.WithSkipExceptions();

        // assert
        retry.ExceptionsToSkip.Should().BeEmpty();
    }

    [Fact]
    public void WithSkipExceptions_SpecificException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        retry.WithSkipExceptions(typeof(TestCanceledException));

        // assert
        retry.ExceptionsToSkip.Should().NotBeEmpty();
        retry.ExceptionsToSkip.Should().OnlyContain(x => x == typeof(TestCanceledException));
    }

    [Fact]
    public void WithSkipExceptions_ThrowException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // assert
        Assert.Throws<ArgumentNullException>(() => retry.WithSkipExceptions(null));
    }
}
