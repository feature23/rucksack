using Microsoft.Extensions.Logging;
using Spectre.Console;

namespace Rucksack;

public class LoadTestOptions
{
    public required ILoadStrategy LoadStrategy { get; init; }

    public ILoggerFactory? LoggerFactory { get; init; }

    public IAnsiConsole Console { get; init; } = AnsiConsole.Console;
}
