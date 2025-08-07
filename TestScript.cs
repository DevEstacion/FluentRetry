using FluentRetry;

var attemptCount = 0;

// Simple test of Fast preset
var result = Retry.Do(() =>
{
    attemptCount++;
    Console.WriteLine($"Attempt {attemptCount}");
    if (attemptCount == 1)
        throw new InvalidOperationException("First attempt fails");
    return "Success";
})
.Fast()
.Execute();

Console.WriteLine($"Result: {result}");
