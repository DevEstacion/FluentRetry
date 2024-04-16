namespace FluentRetry;

[ExcludeFromCodeCoverage]
internal static class RetryInternals
{
    public static RetryConfiguration RetryConfiguration { get; internal set; } = new();
}
