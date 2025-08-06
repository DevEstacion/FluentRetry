namespace FluentRetry;

/// <summary>
/// Represents jitter configuration for adding randomness to retry delays.
/// </summary>
public sealed class Jitter : IEquatable<Jitter>
{
    private static readonly Jitter _none = new(0, 0);

    private Jitter(int low, int high)
    {
        Low = low;
        High = high;
    }

    /// <summary>
    /// The minimum jitter value in milliseconds.
    /// </summary>
    public int Low { get; }

    /// <summary>
    /// The maximum jitter value in milliseconds.
    /// </summary>
    public int High { get; }

    /// <summary>
    /// Gets a jitter configuration with no randomness.
    /// </summary>
    public static Jitter None => _none;

    /// <summary>
    /// Creates a jitter configuration with the specified range.
    /// </summary>
    /// <param name="low">Minimum jitter value in milliseconds (inclusive).</param>
    /// <param name="high">Maximum jitter value in milliseconds (exclusive).</param>
    /// <returns>A new Jitter instance.</returns>
    /// <exception cref="ArgumentOutOfRangeException">Thrown when the range is invalid.</exception>
    public static Jitter Range(int low, int high)
    {
        if (low < 0)
            throw new ArgumentOutOfRangeException(nameof(low), "Low value cannot be negative.");

        if (high <= low)
            throw new ArgumentOutOfRangeException(nameof(high), "High value must be greater than low value.");

        return new Jitter(low, high);
    }

    /// <summary>
    /// Creates a jitter configuration with values from 0 to the specified maximum.
    /// </summary>
    /// <param name="maxJitter">Maximum jitter value in milliseconds (exclusive).</param>
    /// <returns>A new Jitter instance.</returns>
    public static Jitter UpTo(int maxJitter)
    {
        return Range(0, maxJitter);
    }

    /// <summary>
    /// Creates a jitter configuration with a fixed percentage of the base delay.
    /// </summary>
    /// <param name="percentage">Percentage of base delay to use as maximum jitter (0-100).</param>
    /// <param name="baseDelayMs">Base delay in milliseconds to calculate percentage from.</param>
    /// <returns>A new Jitter instance.</returns>
    public static Jitter Percentage(double percentage, int baseDelayMs)
    {
        if (percentage < 0 || percentage > 100)
            throw new ArgumentOutOfRangeException(nameof(percentage), "Percentage must be between 0 and 100.");

        if (baseDelayMs <= 0)
            throw new ArgumentOutOfRangeException(nameof(baseDelayMs), "Base delay must be positive.");

        var maxJitter = (int)(baseDelayMs * percentage / 100);
        return maxJitter > 0 ? Range(0, maxJitter) : None;
    }

    /// <summary>
    /// Generates a random jitter value within the configured range.
    /// </summary>
    /// <returns>Random jitter value in milliseconds.</returns>
    public int GetJitter()
    {
        if (Low == High) return Low;
        return Random.Shared.Next(Low, High);
    }

    /// <summary>
    /// Generates a random jitter value using full jitter strategy (0 to max range).
    /// </summary>
    /// <returns>Random jitter value between 0 and High (exclusive).</returns>
    public int GetFullJitter()
    {
        return High > 0 ? Random.Shared.Next(0, High) : 0;
    }

    public bool Equals(Jitter other)
    {
        if (ReferenceEquals(null, other)) return false;
        if (ReferenceEquals(this, other)) return true;
        return Low == other.Low && High == other.High;
    }

    public override bool Equals(object obj)
    {
        return ReferenceEquals(this, obj) || obj is Jitter other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Low, High);
    }

    public static bool operator ==(Jitter left, Jitter right)
    {
        return Equals(left, right);
    }

    public static bool operator !=(Jitter left, Jitter right)
    {
        return !Equals(left, right);
    }

    public override string ToString()
    {
        return $"Jitter[{Low}, {High})";
    }
}
