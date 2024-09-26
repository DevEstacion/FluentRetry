namespace FluentRetry.Tests;

public class RetryConfigurationTests
{
    [Fact]
    public void Default()
    {
        // act
        var config = new RetryConfiguration();

        // assert
        config.RetrySleepInMs.Should().Be(150);
        config.RetryCount.Should().Be(3);
        config.Jitter.Low.Should().Be(10);
        config.Jitter.High.Should().Be(100);
    }

    [Fact]
    public void Set_All()
    {
        // act
        var config = new RetryConfiguration
        {
            RetrySleepInMs = 1000,
            RetryCount = 10,
            Jitter = Jitter.Range(100, 200)
        };

        // assert
        config.RetrySleepInMs.Should().Be(1000);
        config.RetryCount.Should().Be(10);
        config.Jitter.Low.Should().Be(100);
        config.Jitter.High.Should().Be(200);
    }

    [Fact]
    public void Set_Jitter()
    {
        // act
        var config = new RetryConfiguration
        {
            Jitter = Jitter.Range(100, 200)
        };

        // assert
        config.RetrySleepInMs.Should().Be(150);
        config.RetryCount.Should().Be(3);
        config.Jitter.Low.Should().Be(100);
        config.Jitter.High.Should().Be(200);
    }

    [Fact]
    public void Set_RetryCount()
    {
        // act
        var config = new RetryConfiguration { RetryCount = 10 };

        // assert
        config.RetrySleepInMs.Should().Be(150);
        config.RetryCount.Should().Be(10);
        config.Jitter.Low.Should().Be(10);
        config.Jitter.High.Should().Be(100);
    }

    [Fact]
    public void Set_RetrySleepInMs()
    {
        // act
        var config = new RetryConfiguration { RetrySleepInMs = 1000 };

        // assert
        config.RetrySleepInMs.Should().Be(1000);
        config.RetryCount.Should().Be(3);
        config.Jitter.Low.Should().Be(10);
        config.Jitter.High.Should().Be(100);
    }
}
