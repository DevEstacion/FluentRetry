namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void WithCaller_ThrowException(string caller)
    {
        // arrange
        var retry = new TestRetry(() => { });

        // assert
        Assert.Throws<ArgumentNullException>(() => retry.WithCaller(caller));
    }

    [Fact]
    public void WithCaller()
    {
        // arrange
        const string Caller = "TEST";
        var retry = new TestRetry(() => { });

        // act
        retry.WithCaller(Caller);

        // assert
        retry.Caller.Should().BeEquivalentTo(Caller);
    }
}
