namespace FluentRetry.Tests;

public class GenericRetryTests
{
    public GenericRetryTests()
    {
        RetryInternals.RetryConfiguration = new RetryConfiguration();
    }

    [Fact]
    public void RunAsync()
    {
        // arrange
        var totalInvocation = 0;
        var retry = new GenericRetry<int>(() =>
        {
            totalInvocation++;
            return 50;
        });

        // act
        var result = retry.Run();

        // assert
        totalInvocation.Should().Be(1);
        result.Should().Be(50);
    }

    [Fact]
    public void RunAsync_WithOnResult()
    {
        // arrange
        var totalInvocation = 0;
        var onResultInvocation = 0;
        var onResultParameter = 0;
        var retry = new GenericRetry<int>(() =>
        {
            totalInvocation++;
            return 50;
        }).WithOnResult(result =>
        {
            onResultParameter = result;
            onResultInvocation++;
            return false;
        });

        // act
        var result = retry.Run();

        // assert
        totalInvocation.Should().Be(1);
        onResultParameter.Should().Be(50);
        onResultInvocation.Should().Be(1);
        result.Should().Be(50);
    }
}
