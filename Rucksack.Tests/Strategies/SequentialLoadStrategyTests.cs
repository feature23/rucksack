using FluentAssertions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

[Collection(TestCollections.StrategyTests)]
public class SequentialLoadStrategyTests
{
    [Fact]
    public async Task SequentialLoadStrategy_BasicOneShotTest()
    {
        // Arrange
        const int expectedCount = 12; // [2, 4, 6]
        var actionCalledCount = 0;
        var strategy = new SequentialLoadStrategy(intervalBetweenStrategies: TimeSpan.FromSeconds(1))
        {
            new OneShotLoadStrategy(count: 2),
            new OneShotLoadStrategy(count: 4),
            new OneShotLoadStrategy(count: 6),
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
        const int expectedCount = 45; // [1, 2, 3, 4, 5, 5, 5, 5, 5, 4, 3, 2, 1]
        var actionCalledCount = 0;
        var strategy = new SequentialLoadStrategy(intervalBetweenStrategies: TimeSpan.FromSeconds(1))
        {
            new SteppedBurstLoadStrategy(step: 1, from: 1, to: 5, interval: TimeSpan.FromSeconds(1)), // [1, 2, 3, 4, 5]
            new RepeatBurstLoadStrategy(countPerInterval: 5, interval: TimeSpan.FromSeconds(1), totalDuration: TimeSpan.FromSeconds(3)), // [5, 5, 5]
            new SteppedBurstLoadStrategy(step: -1, from: 5, to: 1, interval: TimeSpan.FromSeconds(1)), // [5, 4, 3, 2, 1]
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
