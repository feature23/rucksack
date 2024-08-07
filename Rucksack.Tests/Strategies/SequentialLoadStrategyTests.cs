using FluentAssertions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

public class SequentialLoadStrategyTests
{
    [Fact]
    public async Task SequentialLoadStrategy_BasicOneShotTest()
    {
        // Arrange
        const int expectedCount = 60; // [10, 20, 30]
        var actionCalledCount = 0;
        var strategy = new SequentialLoadStrategy(intervalBetweenStrategies: TimeSpan.FromSeconds(1))
        {
            new OneShotLoadStrategy(count: 10),
            new OneShotLoadStrategy(count: 20),
            new OneShotLoadStrategy(count: 30),
        };

        LoadTask action = () =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.FromResult(new LoadTaskResult(TimeSpan.Zero));
        };

        // Act
        await StrategyTestHelper.RunFullStrategyTest(strategy, action);

        // Assert
        actionCalledCount.Should().Be(expectedCount);
    }

    [Fact]
    public async Task SequentialLoadStrategy_BasicSteppedBurstTest()
    {
        // Arrange
        const int expectedCount = 450; // [10, 20, 30, 40, 50, 50, 50, 50, 50, 40, 30, 20, 10]
        var actionCalledCount = 0;
        var strategy = new SequentialLoadStrategy(intervalBetweenStrategies: TimeSpan.FromSeconds(1))
        {
            new SteppedBurstLoadStrategy(step: 10, from: 10, to: 50, interval: TimeSpan.FromSeconds(1)), // [10, 20, 30, 40, 50]
            new RepeatBurstLoadStrategy(countPerInterval: 50, interval: TimeSpan.FromSeconds(1), totalDuration: TimeSpan.FromSeconds(3)), // [50, 50, 50]
            new SteppedBurstLoadStrategy(step: -10, from: 50, to: 10, interval: TimeSpan.FromSeconds(1)), // [50, 40, 30, 20, 10]
        };

        LoadTask action = () =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.FromResult(new LoadTaskResult(TimeSpan.Zero));
        };

        // Act
        await StrategyTestHelper.RunFullStrategyTest(strategy, action);

        // Assert
        actionCalledCount.Should().Be(expectedCount);
    }
}
