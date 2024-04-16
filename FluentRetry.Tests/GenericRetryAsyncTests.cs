namespace FluentRetry.Tests;

public class GenericRetryAsyncTests
{
    public GenericRetryAsyncTests()
    {
        RetryInternals.RetryConfiguration = new RetryConfiguration();
    }

    [Fact]
    public async Task RunAsync()
    {
        // arrange
        var totalInvocation = 0;
        var retry = new GenericRetryAsync<int>(() =>
        {
            totalInvocation++;
            return Task.FromResult(50);
        });

        // act
        var result = await retry.Run();

        // assert
        totalInvocation.Should().Be(1);
        result.Should().Be(50);
    }

    [Fact]
    public async Task RunAsync_WithOnResult()
    {
        // arrange
        var totalInvocation = 0;
        var onResultInvocation = 0;
        var onResultParameter = 0;
        var retry = new GenericRetryAsync<int>(() =>
        {
            totalInvocation++;
            return Task.FromResult(50);
        }).WithOnResult(result =>
        {
            onResultParameter = result;
            onResultInvocation++;
            return false;
        });

        // act
        var result = await retry.Run();

        // assert
        totalInvocation.Should().Be(1);
        onResultParameter.Should().Be(50);
        onResultInvocation.Should().Be(1);
        result.Should().Be(50);
    }
}
