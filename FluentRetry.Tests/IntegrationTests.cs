using System.Collections.Concurrent;

namespace FluentRetry.Tests;

public class IntegrationTests
{
    [Fact]
    public void RealWorldScenario_HttpClientSimulation_HandlesFailuresCorrectly()
    {
        // Arrange
        var attemptCount = 0;
        var httpResponseCodes = new[] { 500, 503, 200 }; // Fail twice, then succeed

        var httpSimulation = () =>
        {
            var responseCode = httpResponseCodes[Math.Min(attemptCount, httpResponseCodes.Length - 1)];
            attemptCount++;

            if (responseCode >= 500)
                throw new HttpRequestException($"HTTP {responseCode} error");

            return $"Success with HTTP {responseCode}";
        };

        // Act
        var result = Retry.Do(httpSimulation)
            .Network()
            .OnRetry((ex, attempt) =>
            {
                // Should be called twice (for the 500 and 503 errors)
            })
            .Execute();

        // Assert
        result.Should().Be("Success with HTTP 200");
        attemptCount.Should().Be(3);
    }

    [Fact]
    public async Task RealWorldScenario_DatabaseConnectionSimulation_HandlesTimeoutsCorrectly()
    {
        // Arrange
        var attemptCount = 0;
        var timeoutOccursOn = new[] { 1, 2 }; // Timeout on first two attempts

        var databaseSimulation = async () =>
        {
            await Task.Delay(1); // Simulate async database call
            attemptCount++;

            if (timeoutOccursOn.Contains(attemptCount))
                throw new TimeoutException("Database connection timeout");

            return new { UserId = 123, Name = "John Doe" };
        };

        // Act
        var result = await Retry.DoAsync(databaseSimulation)
            .Database()
            .OnRetry((ex, attempt) =>
            {
                ex.Should().BeOfType<TimeoutException>();
            })
            .ExecuteAsync();

        // Assert
        result.Should().NotBeNull();
        result!.UserId.Should().Be(123);
        result.Name.Should().Be("John Doe");
        attemptCount.Should().Be(3);
    }

    [Fact]
    public void RealWorldScenario_FileReadSimulation_HandlesFileLockCorrectly()
    {
        // Arrange
        var attemptCount = 0;
        var fileLockReleasedAfter = 2;

        var fileReadSimulation = () =>
        {
            attemptCount++;

            if (attemptCount <= fileLockReleasedAfter)
                throw new IOException("File is locked by another process");

            return "File content successfully read";
        };

        // Act
        var result = Retry.Do(fileReadSimulation)
            .Attempts(3)
            .Delay(1) // Fast test
            .OnRetry((ex, attempt) =>
            {
                ex.Should().BeOfType<IOException>();
            })
            .Execute();

        // Assert
        result.Should().Be("File content successfully read");
        attemptCount.Should().Be(3);
    }

    [Fact]
    public void ComplexScenario_MixedFailureTypesWithCustomLogic()
    {
        // Arrange
        var attemptCount = 0;
        var retryLog = new List<string>();
        var failureLog = new List<string>();

        var complexOperation = () =>
        {
            attemptCount++;

            switch (attemptCount)
            {
                case 1:
                    throw new UnauthorizedAccessException("Authentication failed");
                case 2:
                    throw new TimeoutException("Request timeout");
                case 3:
                    return 0; // Invalid result that should trigger retry condition
                case 4:
                    return 42; // Success
                default:
                    throw new InvalidOperationException("Unexpected attempt");
            }
        };

        // Act
        var result = Retry.Do(complexOperation)
            .Attempts(5)
            .Delay(1)
            .RetryWhen(value => value == 0)
            .OnRetry((ex, attempt) =>
            {
                retryLog.Add($"Attempt {attempt}: {ex.GetType().Name}");
            })
            .OnFailure(ex =>
            {
                failureLog.Add($"Final failure: {ex.GetType().Name}");
            })
            .Execute();

        // Assert
        result.Should().Be(42);
        attemptCount.Should().Be(4);
        retryLog.Should().HaveCount(3); // Three retries
        retryLog[0].Should().Contain("UnauthorizedAccessException");
        retryLog[1].Should().Contain("TimeoutException");
        retryLog[2].Should().Contain("InvalidOperationException"); // From retry condition
        failureLog.Should().BeEmpty(); // Should not fail
    }

    [Fact]
    public async Task ConcurrencyScenario_MultipleSimultaneousRetries()
    {
        // Arrange
        var globalCounter = 0;
        var results = new ConcurrentBag<int>();

        var concurrentOperation = async (int operationId) =>
        {
            await Task.Delay(Random.Shared.Next(1, 10)); // Random delay
            var localAttempt = Interlocked.Increment(ref globalCounter);

            // Fail sometimes to test retry logic
            if (localAttempt % 3 == 0)
                throw new InvalidOperationException($"Operation {operationId} failed on attempt {localAttempt}");

            return operationId * 100 + localAttempt;
        };

        // Act
        var tasks = Enumerable.Range(1, 5).Select(async operationId =>
        {
            var result = await Retry.DoAsync(() => concurrentOperation(operationId))
                .Attempts(5)
                .Delay(1)
                .ExecuteAsync();
            results.Add(result);
            return result;
        });

        await Task.WhenAll(tasks);

        // Assert
        results.Should().HaveCount(5);
        results.Should().AllSatisfy(r => r.Should().BeGreaterThan(0));
        globalCounter.Should().BeGreaterThan(5); // Some operations should have retried
    }

    [Fact]
    public void PerformanceScenario_ManyQuickRetries_CompletesInReasonableTime()
    {
        // Arrange
        var operationCount = 100;
        var stopwatch = System.Diagnostics.Stopwatch.StartNew();

        // Act
        Parallel.For(0, operationCount, i =>
        {
            var operationIndex = i;
            var attemptCount = 0;
            var result = Retry.Do(() =>
            {
                attemptCount++;
                if (operationIndex % 10 == 0 && attemptCount == 1) // Fail every 10th operation only on first attempt
                    throw new InvalidOperationException("Simulated failure");
                return operationIndex;
            })
            .Fast()
            .Execute();

            result.Should().Be(operationIndex);
        });

        stopwatch.Stop();

        // Assert
        stopwatch.ElapsedMilliseconds.Should().BeLessThan(5000); // Should complete in under 5 seconds
    }

    [Fact]
    public void ChainedConfigurationScenario_OverridingSettings()
    {
        // Arrange
        var attemptCount = 0;
        var delays = new List<long>();
        var lastTime = DateTimeOffset.UtcNow;

        var operation = () =>
        {
            var now = DateTimeOffset.UtcNow;
            if (attemptCount > 0)
            {
                delays.Add((now - lastTime).Milliseconds);
            }
            lastTime = now;
            attemptCount++;
            throw new InvalidOperationException("Always fails");
        };

        // Act
        Retry.Do(operation)
            .Fast() // 2 attempts, 50ms delay
            .Attempts(4) // Override to 4 attempts
            .Delay(20) // Override to 20ms delay
            .WithExponentialBackoff() // Add exponential backoff
            .WithJitter(0) // Remove jitter for predictable timing
            .Execute();

        // Assert
        attemptCount.Should().Be(4); // Should use overridden attempts
        delays.Should().HaveCount(3);
        // Should have exponential backoff with 20ms base delay
        delays[1].Should().BeGreaterThan(delays[0]);
    }

    [Fact]
    public async Task ExtensionMethodsIntegration_WorkWithComplexTypes()
    {
        // Arrange
        var dataRepository = new TestDataRepository();

        // Act & Assert - Action extension
        dataRepository.SaveData.WithRetry(3);
        dataRepository.SaveCallCount.Should().Be(2); // Fails once, succeeds on second

        // Act & Assert - Function extension
        var data = dataRepository.GetData.WithRetry(3);
        data.Should().NotBeNull();
        data!.Id.Should().Be(42);

        // Act & Assert - Async extensions
        var asyncTask = async () => await dataRepository.SaveDataAsync();
        await asyncTask.WithRetryAsync(3);
        dataRepository.AsyncSaveCallCount.Should().Be(2);

        var asyncResult = await dataRepository.GetDataAsync.WithRetryAsync(3);
        asyncResult.Should().NotBeNull();
        asyncResult!.Name.Should().Be("Test Data");
    }

    private class TestDataRepository
    {
        public int SaveCallCount { get; private set; }
        public int AsyncSaveCallCount { get; private set; }

        public Action SaveData => () =>
        {
            SaveCallCount++;
            if (SaveCallCount == 1)
                throw new InvalidOperationException("Save failed");
        };

        public Func<TestData?> GetData => () =>
        {
            if (SaveCallCount == 0) // First call fails
            {
                SaveCallCount++; // Track calls
                throw new InvalidOperationException("Get failed");
            }
            return new TestData { Id = 42, Name = "Test Data" };
        };

        public Func<Task> SaveDataAsync => async () =>
        {
            await Task.Delay(1);
            AsyncSaveCallCount++;
            if (AsyncSaveCallCount == 1)
                throw new InvalidOperationException("Async save failed");
        };

        public Func<Task<TestData?>> GetDataAsync => async () =>
        {
            await Task.Delay(1);
            return new TestData { Id = 24, Name = "Test Data" };
        };
    }

    private class TestData
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty;
    }
}
