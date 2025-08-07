using FluentRetry;

// Test function vs action resolution
var testValue = 42;

// This should work - explicit Func<int>
Func<int> explicitFunc = () => testValue;
var result1 = Retry.Do(explicitFunc).Execute();
Console.WriteLine($"Explicit func result: {result1}");

// This might not work - lambda could be ambiguous
var result2 = Retry.Do(() => testValue).Execute();
Console.WriteLine($"Lambda result: {result2}");
