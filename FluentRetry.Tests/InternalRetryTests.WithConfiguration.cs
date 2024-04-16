namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void WithConfiguration()
    {
        // arrange
        var retry = new TestRetry(() => { });
        var retryConfiguration = new RetryConfiguration();

        // act
        retry.WithConfiguration(retryConfiguration);

        // assert
        retry.RetryConfiguration.Should().BeSameAs(retryConfiguration);
    }

    [Fact]
    public void WithConfiguration_ThrowException()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // assert
        Assert.Throws<ArgumentNullException>(() => retry.WithConfiguration(null));
    }
}
