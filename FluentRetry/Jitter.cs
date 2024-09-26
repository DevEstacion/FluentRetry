namespace FluentRetry;

public class Jitter
{
    private Jitter(int low, int high)
    {
        Low = low;
        High = high;
    }

    public int Low { get; }
    public int High { get; }

    public static Jitter Range(int low, int high)
    {
        if (low < 0 || high <= low)
        {
            throw new InvalidOperationException("Range provided is invalid.");
        }

        return new Jitter(low, high);
    }
}
