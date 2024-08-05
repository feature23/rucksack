using Microsoft.Extensions.Logging;

namespace Rucksack;

public class LoadTestOptions
{
    public required ILoadStrategy LoadStrategy { get; init; }

    public ILoggerFactory? LoggerFactory { get; init; }
}
