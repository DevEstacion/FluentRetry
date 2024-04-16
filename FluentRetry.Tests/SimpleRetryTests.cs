namespace FluentRetry.Tests;

public class SimpleRetryTests
{
    public SimpleRetryTests()
    {
        RetryInternals.RetryConfiguration = new RetryConfiguration();
    }

    [Fact]
    public void Run()
    {
        // arrange
        var totalInvocation = 0;
        var retry = new SimpleRetry(() => totalInvocation++);

        // act
        retry.Run();

        // assert
        totalInvocation.Should().Be(1);
    }
}
