namespace FluentRetry;

[ExcludeFromCodeCoverage]
public class RetryConfiguration
{
    private readonly Tuple<int, int> _jitterRange;

    public RetryConfiguration()
    {
        JitterRange = new Tuple<int, int>(10, 100);
    }

    public int RetryCount { get; init; } = 3;
    public int RetrySleepInMs { get; init; } = 150;

    /// <summary>
    ///     Sets the range of the jitter added to each retry
    /// </summary>
    public Tuple<int, int> JitterRange
    {
        get => _jitterRange;
        init
        {
            var low = value.Item1 < 0 ? 0 : value.Item1;
            var high = value.Item2 <= value.Item1 ? value.Item2 + 10 : value.Item2;
            _jitterRange = new Tuple<int, int>(low, high);
        }
    }
}
