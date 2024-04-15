namespace FluentRetry;

[ExcludeFromCodeCoverage]
public static class Retry
{
    public static GenericRetry<T> Create<T>(Task<T> runner)
    {
        return new GenericRetry<T>(runner);
    }

    public static GenericRetry<T> Create<T>(Func<T> runner)
    {
        return new GenericRetry<T>(runner);
    }

    public static SimpleRetry Create(Task runner)
    {
        return new SimpleRetry(runner);
    }

    public static SimpleRetry Create(Action runner)
    {
        return new SimpleRetry(runner);
    }
}
