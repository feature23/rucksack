using FluentAssertions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

[Collection(TestCollections.StrategyTests)]
public class OneShotLoadStrategyTests
{
    [Fact]
    public async Task Step_WithCountOf1_CallsActionOnce()
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new OneShotLoadStrategy(1);
        var context = new LoadStrategyContext(PreviousResult: null, CurrentRunningTasks: 0);

        // Act
        var result = strategy.GenerateLoad(() =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.FromResult(new LoadTaskResult(TimeSpan.Zero));
        }, context);

        await StrategyTestHelper.ExecuteStrategyResultAndWait(result);

        // Assert
        actionCalledCount.Should().Be(1);
        result.RepeatDelay.Should().BeNull();
    }

    [Fact]
    public async Task Step_WithCountOf3_CallsActionThreeTimes()
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new OneShotLoadStrategy(3);
        var context = new LoadStrategyContext(PreviousResult: null, CurrentRunningTasks: 0);

        // Act
        var result = strategy.GenerateLoad(() =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.FromResult(new LoadTaskResult(TimeSpan.Zero));
        }, context);

        await StrategyTestHelper.ExecuteStrategyResultAndWait(result);

        // Assert
        actionCalledCount.Should().Be(3);
        result.RepeatDelay.Should().BeNull();
    }
}
