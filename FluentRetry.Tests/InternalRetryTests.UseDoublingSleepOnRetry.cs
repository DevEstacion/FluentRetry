namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Fact]
    public void UseDoublingSleepOnRetry()
    {
        // arrange
        var retry = new TestRetry(() => { });

        // act
        retry.UseDoublingSleepOnRetry();

        // assert
        retry.DoublingSleepOnRetry.Should().BeTrue();
    }

    [Fact]
    public void UseDoublingSleepOnRetry_Default()
    {
        // act
        var retry = new TestRetry(() => { });

        // assert
        retry.DoublingSleepOnRetry.Should().BeFalse();
    }
}
