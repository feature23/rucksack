using Microsoft.Extensions.Logging;
using Spectre.Console;
using Xunit.Abstractions;

namespace Rucksack.Tests.Util;

public static class LoadTestOptionsFactory
{
    public static LoadTestOptions Create(ILoadStrategy strategy, ITestOutputHelper testOutputHelper)
        => new()
        {
            LoadStrategy = strategy,
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
            Console = AnsiConsole.Create(new AnsiConsoleSettings
            {
                Ansi = AnsiSupport.No,
                ColorSystem = ColorSystemSupport.NoColors,
                Out = new AnsiConsoleOutput(new TestOutputHelperTextWriterAdapter(testOutputHelper)),
            })
        };
}
