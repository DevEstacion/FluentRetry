namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void WithExceptions_ThrowException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // assert
        Assert.Throws<ArgumentNullException>(() => retry.WithOnException(null));
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
}
