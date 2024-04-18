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
        config.JitterRange.Item1.Should().Be(10);
        config.JitterRange.Item2.Should().Be(100);
    }

    [Fact]
    public void Set_All()
    {
        // act
        var config = new RetryConfiguration
        {
            RetrySleepInMs = 1000,
            RetryCount = 10,
            JitterRange = new Tuple<int, int>(100, 200)
        };

        // assert
        config.RetrySleepInMs.Should().Be(1000);
        config.RetryCount.Should().Be(10);
        config.JitterRange.Item1.Should().Be(100);
        config.JitterRange.Item2.Should().Be(200);
    }

    [Fact]
    public void Set_JitterRange()
    {
        // act
        var config = new RetryConfiguration
        {
            JitterRange = new Tuple<int, int>(100, 200)
        };

        // assert
        config.RetrySleepInMs.Should().Be(150);
        config.RetryCount.Should().Be(3);
        config.JitterRange.Item1.Should().Be(100);
        config.JitterRange.Item2.Should().Be(200);
    }

    [Fact]
    public void Set_RetryCount()
    {
        // act
        var config = new RetryConfiguration { RetryCount = 10 };

        // assert
        config.RetrySleepInMs.Should().Be(150);
        config.RetryCount.Should().Be(10);
        config.JitterRange.Item1.Should().Be(10);
        config.JitterRange.Item2.Should().Be(100);
    }

    [Fact]
    public void Set_RetrySleepInMs()
    {
        // act
        var config = new RetryConfiguration { RetrySleepInMs = 1000 };

        // assert
        config.RetrySleepInMs.Should().Be(1000);
        config.RetryCount.Should().Be(3);
        config.JitterRange.Item1.Should().Be(10);
        config.JitterRange.Item2.Should().Be(100);
    }
}
