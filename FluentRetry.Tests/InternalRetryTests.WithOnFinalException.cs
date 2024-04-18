namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void WithOnFinalException()
    {
        // arrange
        var retry = new TestRetry(() => { });
        Action<RetryContext> onExceptionRunner = _ => { };

        // act
        retry.WithOnFinalException(onExceptionRunner);

        // assert
        retry.OnFinalExceptionRunner.Should().BeSameAs(onExceptionRunner);
    }

    [Fact]
    public void WithOnFinalException_ThrowException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // assert
        Assert.Throws<ArgumentNullException>(() => retry.WithOnFinalException(null));
    }
}
