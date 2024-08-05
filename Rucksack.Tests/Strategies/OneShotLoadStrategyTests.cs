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

        // Act
        var result = strategy.Step(() =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.CompletedTask;
        }, null);

        await StrategyHelper.ExecuteStrategyResultAndWait(result);

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

        // Act
        var result = strategy.Step(() =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.CompletedTask;
        }, null);

        await StrategyHelper.ExecuteStrategyResultAndWait(result);

        // Assert
        actionCalledCount.Should().Be(3);
        result.NextStepDelay.Should().BeNull();
    }
}
