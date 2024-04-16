using Microsoft.VisualStudio.TestPlatform.ObjectModel.Adapter;

namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void WithExceptions_ThrowException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // assert
        Assert.Throws<ArgumentNullException>(() => retry.WithOnException(null, typeof(TestCanceledException)));
        Assert.Throws<ArgumentNullException>(() => retry.WithOnException(_ => { }, null));
    }

    [Fact]
    public void WithOnExceptions()
    {
        // arrange
        var retry = new TestRetry(() => { });
        Action<RetryContext> onExceptionRunner = _ => { };

        // act
        retry.WithOnException(onExceptionRunner);

        // assert
        retry.OnExceptionRunner.Should().BeSameAs(onExceptionRunner);
    }

    [Fact]
    public void WithOnExceptions_DuplicateException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        retry.WithOnException(_ => { }, typeof(TestCanceledException), typeof(TestCanceledException));

        // assert
        retry.ExceptionToHandle.Should().NotBeEmpty();
        retry.ExceptionToHandle.Should().OnlyContain(x => x == typeof(TestCanceledException));
    }

    [Fact]
    public void WithOnExceptions_EmptyException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        retry.WithOnException(_ => { });

        // assert
        retry.ExceptionToHandle.Should().NotBeEmpty();
        retry.ExceptionToHandle.Should().OnlyContain(x => x == typeof(Exception));

        // act
        retry.WithOnException(_ => { }, Array.Empty<Type>());

        // assert
        retry.ExceptionToHandle.Should().NotBeEmpty();
        retry.ExceptionToHandle.Should().OnlyContain(x => x == typeof(Exception));
    }

    [Fact]
    public void WithOnExceptions_SpecificException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        retry.WithOnException(_ => { }, typeof(TestCanceledException));

        // assert
        retry.ExceptionToHandle.Should().NotBeEmpty();
        retry.ExceptionToHandle.Should().OnlyContain(x => x == typeof(TestCanceledException));
    }
}
