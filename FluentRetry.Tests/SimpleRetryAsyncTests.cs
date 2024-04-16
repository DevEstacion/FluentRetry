namespace FluentRetry.Tests;

public class SimpleRetryAsyncTests
{
    public SimpleRetryAsyncTests()
    {
        Retry.RetryConfiguration = new RetryConfiguration();
    }

    [Fact]
    public async Task RunAsync()
    {
        // arrange
        var totalInvocation = 0;
        var retry = new SimpleRetryAsync(() =>
        {
            totalInvocation++;
            return Task.CompletedTask;
        });

        // act
        await retry.Run();

        // assert
        totalInvocation.Should().Be(1);
    }
}
