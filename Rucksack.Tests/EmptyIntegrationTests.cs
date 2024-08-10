using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rucksack.LoadStrategies;
using Rucksack.Tests.Util;
using Spectre.Console;
using Xunit.Abstractions;

namespace Rucksack.Tests;

public class EmptyIntegrationTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public void EmptyIntegrationTest()
    {
        bool executed = false;

        LoadTestRunner.Run(() =>
        {
            executed = true;
        }, LoadTestOptionsFactory.Create(new OneShotLoadStrategy(1), testOutputHelper));

        executed.Should().BeTrue();
    }

    [Fact]
    public async Task AsyncIntegrationTest()
    {
        bool executed = false;

        await LoadTestRunner.Run(async () =>
        {
            await Task.Delay(100);
            executed = true;
        }, LoadTestOptionsFactory.Create(new OneShotLoadStrategy(1), testOutputHelper));

        executed.Should().BeTrue();
    }
}
