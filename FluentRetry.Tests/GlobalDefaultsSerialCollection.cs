using Xunit;

namespace FluentRetry.Tests;

// Marker collection definition to serialize tests that manipulate global defaults
[CollectionDefinition("GlobalDefaultsSerial", DisableParallelization = true)]
public class GlobalDefaultsSerialCollection {}
