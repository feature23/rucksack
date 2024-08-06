using FluentAssertions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

public class OneShotLoadStrategyTests
{
    [Fact]
    public async Task Step_WithCountOf1_CallsActionOnce()
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new OneShotLoadStrategy(1);
        var context = new LoadStrategyContext(PreviousResult: null);

        // Act
        var result = strategy.Step(() =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return ValueTask.FromResult(new LoadTaskResult(TimeSpan.Zero));
        }, context);

        await StrategyTestHelper.ExecuteStrategyResultAndWait(result);

        // Assert
        actionCalledCount.Should().Be(1);
        result.NextStepDelay.Should().BeNull();
    }

    [Fact]
    public async Task Step_WithCountOf3_CallsActionThreeTimes()
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new OneShotLoadStrategy(3);
        var context = new LoadStrategyContext(PreviousResult: null);

        // Act
        var result = strategy.Step(() =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return ValueTask.FromResult(new LoadTaskResult(TimeSpan.Zero));
        }, context);

        await StrategyTestHelper.ExecuteStrategyResultAndWait(result);

        // Assert
        actionCalledCount.Should().Be(3);
        result.NextStepDelay.Should().BeNull();
    }
}
