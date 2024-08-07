using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rucksack.LoadStrategies;
using Xunit.Abstractions;
using static System.TimeSpan;

namespace Rucksack.Tests;

public class BasicIntegrationTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task BasicOneShotIntegrationTest()
    {
        int executionCount = 0;
        const int count = 1000;

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, new LoadTestOptions
        {
            LoadStrategy = new OneShotLoadStrategy(count),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(count);
    }

    [Fact]
    public async Task BasicRepeatIntegrationTest()
    {
        int executionCount = 0;
        const int count = 10;
        const int intervalSeconds = 1;
        const int durationSeconds = 10;

        var interval = FromSeconds(intervalSeconds);
        var duration = FromSeconds(durationSeconds);

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, new LoadTestOptions
        {
            LoadStrategy = new RepeatLoadStrategy(count, interval, duration),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(count * durationSeconds / intervalSeconds);
    }

    [Fact]
    public async Task BasicSteppedBurstIntegrationTest()
    {
        int executionCount = 0;
        const int step = 10;
        const int from = 10;
        const int to = 50;
        const int expected = 150; // 10 + 20 + 30 + 40 + 50

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, new LoadTestOptions
        {
            LoadStrategy = new SteppedBurstLoadStrategy(step, from, to, FromSeconds(1)),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task RandomDelayRepeatIntegrationTest()
    {
        int executionCount = 0;
        const int count = 10;
        const int intervalSeconds = 1;
        const int durationSeconds = 5;

        var interval = FromSeconds(intervalSeconds);
        var duration = FromSeconds(durationSeconds);
        var random = new Random();

        await LoadTestRunner.Run(async () =>
        {
            var delay = random.Next(1000, 5000);
            await Task.Delay(delay);
            Interlocked.Increment(ref executionCount);
        }, new LoadTestOptions
        {
            LoadStrategy = new RepeatLoadStrategy(count, interval, duration),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(count * durationSeconds / intervalSeconds);
    }
}
