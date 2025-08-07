using Xunit;

// Enable parallel test execution. MaxParallelThreads = 0 lets xUnit choose based on CPU count.
[assembly: CollectionBehavior(DisableTestParallelization = false, MaxParallelThreads = 0)]
