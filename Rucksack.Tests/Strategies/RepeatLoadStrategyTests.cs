using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

public class RepeatLoadStrategyTests
{
    [Fact]
    public async Task Step_WithCountOf1_CallsActionOnce()
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new RepeatLoadStrategy(1, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        var action = () =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.CompletedTask;
        };

        // Act
        var result = strategy.Step(action, null);

        var tasks = await StrategyHelper.ExecuteStrategyResult(result);

        // Assert
        result.NextStepDelay.Should().Be(TimeSpan.FromSeconds(1));

        // Call again
        result = strategy.Step(action, result);

        tasks.AddRange(await StrategyHelper.ExecuteStrategyResult(result));
        await Task.WhenAll(tasks);

        // Assert
        actionCalledCount.Should().Be(1);
        result.NextStepDelay.Should().BeNull();
    }

    [Fact]
    public async Task Step_WithCountOf3_CallsActionThreeTimes()
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new RepeatLoadStrategy(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        var action = () =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.CompletedTask;
        };

        // Act
        var result = strategy.Step(action, null);

        var tasks = await StrategyHelper.ExecuteStrategyResult(result);

        // Assert
        result.NextStepDelay.Should().Be(TimeSpan.FromSeconds(1));

        // Call again
        result = strategy.Step(action, result);

        tasks.AddRange(await StrategyHelper.ExecuteStrategyResult(result));
        await Task.WhenAll(tasks);

        // Assert
        actionCalledCount.Should().Be(3);
        result.NextStepDelay.Should().BeNull();
    }
}
