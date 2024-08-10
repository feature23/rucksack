using FluentAssertions;
using Microsoft.Extensions.Logging;
using Rucksack.LoadStrategies;
using Rucksack.Tests.Util;
using Xunit.Abstractions;
using static System.TimeSpan;

namespace Rucksack.Tests;

public class BasicIntegrationTests(ITestOutputHelper testOutputHelper)
{
    [Fact]
    public async Task BasicOneShotIntegrationTest()
    {
        int executionCount = 0;
        const int count = 250;

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, LoadTestOptionsFactory.Create(new OneShotLoadStrategy(count), testOutputHelper));

        executionCount.Should().Be(count);
    }

    [Fact]
    public async Task BasicRepeatIntegrationTest()
    {
        int executionCount = 0;
        const int count = 5;
        const int intervalSeconds = 1;
        const int durationSeconds = 10;

        var interval = FromSeconds(intervalSeconds);
        var duration = FromSeconds(durationSeconds);

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, LoadTestOptionsFactory.Create(new RepeatBurstLoadStrategy(count, interval, duration), testOutputHelper));

        executionCount.Should().Be(count * durationSeconds / intervalSeconds);
    }

    [Fact]
    public async Task BasicSteppedBurstIntegrationTest()
    {
        int executionCount = 0;
        const int step = 5;
        const int from = 5;
        const int to = 25;
        const int expected = 75; // 5 + 10 + 15 + 20 + 25

        await LoadTestRunner.Run(() =>
            {
                Interlocked.Increment(ref executionCount);
                return Task.CompletedTask;
            },
            LoadTestOptionsFactory.Create(new SteppedBurstLoadStrategy(step, from, to, FromSeconds(1)),
                testOutputHelper));

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicConstantUserLoadIntegrationTest()
    {
        // NOTE: these tests should all finish in under a second,
        // so we can expect this strategy to spawn 10 each second

        int executionCount = 0;
        const int count = 5;
        const int expected = 25;

        await LoadTestRunner.Run(() =>
            {
                Interlocked.Increment(ref executionCount);
                return Task.CompletedTask;
            },
            LoadTestOptionsFactory.Create(new ConstantUserLoadStrategy(count, FromSeconds(1), FromSeconds(5)),
                testOutputHelper));

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicConstantUserLoadIntegrationTest_WithLongRunningTask()
    {
        // Only the first task will take a while to run, so we would
        // expect it to launch in a pattern of [5, 4, 4, 4, 4] for a total of 21

        int executionCount = 0;
        const int count = 5;
        const int expected = 21;

        await LoadTestRunner.Run(async () =>
            {
                int value = Interlocked.Increment(ref executionCount);

                if (value == 1)
                {
                    await Task.Delay(5000);
                }
            },
            LoadTestOptionsFactory.Create(new ConstantUserLoadStrategy(count, FromSeconds(1), FromSeconds(5)),
                testOutputHelper));

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicSteppedUserLoadIntegrationTest()
    {
        // NOTE: these tests should all finish in under a second,
        // so we can expect this strategy to spawn 10-50 each second

        int executionCount = 0;
        const int step = 5;
        const int from = 5;
        const int to = 25;
        const int expected = 75;

        await LoadTestRunner.Run(() =>
            {
                Interlocked.Increment(ref executionCount);
                return Task.CompletedTask;
            },
            LoadTestOptionsFactory.Create(
                new SteppedUserLoadStrategy(step, from, to, FromSeconds(1), FromSeconds(1), FromSeconds(5)),
                testOutputHelper));

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicSteppedUserLoadIntegrationTest_WithLongRunningTask()
    {
        // Only the first task will take a while to run, so we would
        // expect it to launch in a pattern of [5, 9, 14, 19, 24] for a total of 71

        int executionCount = 0;
        const int step = 5;
        const int from = 5;
        const int to = 25;
        const int expected = 71;

        await LoadTestRunner.Run(async () =>
            {
                int value = Interlocked.Increment(ref executionCount);

                if (value == 1)
                {
                    await Task.Delay(5000);
                }
            },
            LoadTestOptionsFactory.Create(
                new SteppedUserLoadStrategy(step, from, to, FromSeconds(1), FromSeconds(1), FromSeconds(5)),
                testOutputHelper));

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task BasicSequentialUserLoadIntegrationTest_OneShot()
    {
        int executionCount = 0;
        const int count = 5;
        const int expected = 25;

        await LoadTestRunner.Run(() =>
        {
            Interlocked.Increment(ref executionCount);
            return Task.CompletedTask;
        }, LoadTestOptionsFactory.Create(new SequentialLoadStrategy(FromSeconds(1))
        {
            new OneShotLoadStrategy(count),
            new OneShotLoadStrategy(count),
            new OneShotLoadStrategy(count),
            new OneShotLoadStrategy(count),
            new OneShotLoadStrategy(count),
        }, testOutputHelper));

        executionCount.Should().Be(expected);
    }

    [Fact]
    public async Task RandomDelayRepeatIntegrationTest()
    {
        int executionCount = 0;
        const int count = 5;
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
        }, LoadTestOptionsFactory.Create(new RepeatBurstLoadStrategy(count, interval, duration), testOutputHelper));

        executionCount.Should().Be(count * durationSeconds / intervalSeconds);
    }
}
