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
            LoadStrategy = new RepeatBurstLoadStrategy(count, interval, duration),
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
    public async Task BasicConstantUserLoadIntegrationTest()
    {
        // NOTE: these tests should all finish in under a second,
        // so we can expect this strategy to spawn 10 each second

        int executionCount = 0;
        const int count = 10;
        const int expected = 50;

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, new LoadTestOptions
        {
            LoadStrategy = new ConstantUserLoadStrategy(count, FromSeconds(1), FromSeconds(5)),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicConstantUserLoadIntegrationTest_WithLongRunningTask()
    {
        // Only the first task will take a while to run, so we would
        // expect it to launch in a pattern of [10, 9, 9, 9, 9] for a total of 46

        int executionCount = 0;
        const int count = 10;
        const int expected = 46;

        await LoadTestRunner.Run(async () =>
        {
            int value = Interlocked.Increment(ref executionCount);

            if (value == 1)
            {
                await Task.Delay(5000);
            }
        }, new LoadTestOptions
        {
            LoadStrategy = new ConstantUserLoadStrategy(count, FromSeconds(1), FromSeconds(5)),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicSteppedUserLoadIntegrationTest()
    {
        // NOTE: these tests should all finish in under a second,
        // so we can expect this strategy to spawn 10-50 each second

        int executionCount = 0;
        const int step = 10;
        const int from = 10;
        const int to = 50;
        const int expected = 150;

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, new LoadTestOptions
        {
            LoadStrategy = new SteppedUserLoadStrategy(step, from, to, FromSeconds(1), FromSeconds(1), FromSeconds(5)),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicSteppedUserLoadIntegrationTest_WithLongRunningTask()
    {
        // Only the first task will take a while to run, so we would
        // expect it to launch in a pattern of [10, 19, 29, 39, 49] for a total of 146

        int executionCount = 0;
        const int step = 10;
        const int from = 10;
        const int to = 50;
        const int expected = 146;

        await LoadTestRunner.Run(async () =>
        {
            int value = Interlocked.Increment(ref executionCount);

            if (value == 1)
            {
                await Task.Delay(5000);
            }
        }, new LoadTestOptions
        {
            LoadStrategy = new SteppedUserLoadStrategy(step, from, to, FromSeconds(1), FromSeconds(1), FromSeconds(5)),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicSequentialUserLoadIntegrationTest_OneShot()
    {
        int executionCount = 0;
        const int count = 10;
        const int expected = 50;

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, new LoadTestOptions
        {
            LoadStrategy = new SequentialLoadStrategy(FromSeconds(1))
            {
                new OneShotLoadStrategy(count),
                new OneShotLoadStrategy(count),
                new OneShotLoadStrategy(count),
                new OneShotLoadStrategy(count),
                new OneShotLoadStrategy(count),
            },
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
            LoadStrategy = new RepeatBurstLoadStrategy(count, interval, duration),
            LoggerFactory = LoggerFactory.Create(builder =>
            {
                builder.AddXUnit(testOutputHelper);
            }),
        });

        executionCount.Should().Be(count * durationSeconds / intervalSeconds);
    }
}
