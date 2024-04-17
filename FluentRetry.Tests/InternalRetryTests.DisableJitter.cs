namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void DisableJitter()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        retry.DisableJitter();

        // assert
        retry.JitterEnabled.Should().BeFalse();
    }

    [Fact]
    public void DisableJitter_Default()
    {
        // act
        var retry = new TestRetry(() => { });

        // assert
        retry.JitterEnabled.Should().BeTrue();
    }
}
