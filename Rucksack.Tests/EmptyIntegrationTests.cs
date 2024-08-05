using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rucksack.LoadStrategies;
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
        }, new LoadTestOptions
        {
            LoadStrategy = new OneShotLoadStrategy(1),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

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
        }, new LoadTestOptions
        {
            LoadStrategy = new OneShotLoadStrategy(1),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executed.Should().BeTrue();
    }
}
