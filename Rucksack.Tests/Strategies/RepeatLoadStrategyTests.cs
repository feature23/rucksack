using FluentAssertions;
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
            return ValueTask.FromResult(new LoadTaskResult(TimeSpan.Zero));
        };
        var context = new LoadStrategyContext(PreviousResult: null);

        // Act
        var result = strategy.GenerateLoad(action, context);

        var tasks = await StrategyTestHelper.ExecuteStrategyResult(result);

        // Assert
        result.RepeatDelay.Should().Be(TimeSpan.FromSeconds(1));

        // Call again
        result = strategy.GenerateLoad(action, new LoadStrategyContext(PreviousResult: result));

        tasks.AddRange(await StrategyTestHelper.ExecuteStrategyResult(result));
        await StrategyTestHelper.WhenAll(tasks);

        // Assert
        actionCalledCount.Should().Be(1);
        result.RepeatDelay.Should().BeNull();
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
            return ValueTask.FromResult(new LoadTaskResult(TimeSpan.Zero));
        };
        var context = new LoadStrategyContext(PreviousResult: null);

        // Act
        var result = strategy.GenerateLoad(action, context);

        var tasks = await StrategyTestHelper.ExecuteStrategyResult(result);

        // Assert
        result.RepeatDelay.Should().Be(TimeSpan.FromSeconds(1));

        // Call again
        result = strategy.GenerateLoad(action, new LoadStrategyContext(PreviousResult: result));

        tasks.AddRange(await StrategyTestHelper.ExecuteStrategyResult(result));
        await StrategyTestHelper.WhenAll(tasks);

        // Assert
        actionCalledCount.Should().Be(3);
        result.RepeatDelay.Should().BeNull();
    }
}
