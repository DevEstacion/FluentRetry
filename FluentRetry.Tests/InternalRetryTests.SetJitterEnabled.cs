namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void DisableJitter()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        _ = retry.SetJitterEnabled(false);

        // assert
        retry.JitterEnabled.Should().BeFalse();
    }

    [Fact]
    public void DisableJitter_Default()
    {
        // act
        var retry = new TestRetry(() => { });

        // assert
        _ = retry.JitterEnabled.Should().BeTrue();
    }

    [Fact]
    public void EnableJitter()
    {
        // arrange
        var retry = new TestRetry(() => { });
        retry.SetJitterEnabled(false);
        var oldJitter = retry.JitterEnabled;

        // act
        _ = retry.SetJitterEnabled(true);

        // assert
        oldJitter.Should().BeFalse();
        retry.JitterEnabled.Should().BeTrue();
    }
}
