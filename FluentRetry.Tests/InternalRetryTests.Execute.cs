namespace FluentRetry.Tests;

public partial class InternalRetryTests
{
    public InternalRetryTests()
    {
        Retry.RetryConfiguration = new RetryConfiguration();
    }

    private class TestRetry : InternalRetry<TestRetry>
    {
        private readonly Action _runner;
        private readonly Func<bool> _onResult;

        public TestRetry(Action runner, Func<bool>? onResult = null)
        {
            _runner = runner;
            _onResult = onResult ?? (() => false);
        }

        public void Run()
        {
            Execute().GetAwaiter().GetResult();
        }

        public Task RunAsync()
        {
            return Execute();
        }

        protected internal override Task PerformRunner()
        {
            _runner();
            return Task.CompletedTask;
        }

        protected internal override bool OnResult()
        {
            return _onResult();
        }
    }

    [Fact]
    public void Execute()
    {
        // arrange
        var totalInvocation = 0;
        var retry = new TestRetry(() => totalInvocation++);

        // act
        retry.Run();

        // assert
        totalInvocation.Should().Be(1);
    }

    [Fact]
    public void Execute_ThrowsException()
    {
        // arrange
        var totalInvocation = 0;
        var retry = new TestRetry(() =>
        {
            totalInvocation++;
            throw new Exception();
        });

        // act
        Assert.Throws<Exception>(retry.Run);

        // assert
        totalInvocation.Should().Be(4);
    }

    [Fact]
    public void Execute_ThrowsException_WithOnException()
    {
        // arrange
        var totalInvocation = 0;
        var onExceptionInvocation = 0;
        var retry = new TestRetry(() =>
            {
                totalInvocation++;
                throw new Exception();
            })
            .WithOnException(_ => onExceptionInvocation++);

        // act
        Assert.Throws<Exception>(retry.Run);

        // assert
        totalInvocation.Should().Be(4);
        onExceptionInvocation.Should().Be(3);
    }

    [Fact]
    public void Execute_WithOnResult()
    {
        // arrange
        var totalInvocation = 0;
        var onResultInvocation = 0;
        var retry = new TestRetry(() => totalInvocation++, () =>
        {
            onResultInvocation++;
            return false;
        });

        // act
        retry.Run();

        // assert
        totalInvocation.Should().Be(1);
        onResultInvocation.Should().Be(1);
    }

    [Fact]
    public void Execute_WithOnResult_RetryTrue()
    {
        // arrange
        var totalInvocation = 0;
        var onResultInvocation = 0;
        var retry = new TestRetry(() => totalInvocation++, () =>
        {
            onResultInvocation++;
            return true;
        });

        // act
        Assert.Throws<Exception>(retry.Run);

        // assert
        totalInvocation.Should().Be(4);
        onResultInvocation.Should().Be(4);
    }

    [Fact]
    public void Execute_WithOnResult_RetryTrue_WithOnException()
    {
        // arrange
        var totalInvocation = 0;
        var onResultInvocation = 0;
        var onExceptionInvocation = 0;
        var retry = new TestRetry(() => totalInvocation++, () =>
            {
                onResultInvocation++;
                return true;
            })
            .WithOnException(_ => onExceptionInvocation++);

        // act
        Assert.Throws<Exception>(retry.Run);

        // assert
        totalInvocation.Should().Be(4);
        onResultInvocation.Should().Be(4);
        onExceptionInvocation.Should().Be(3);
    }
}
