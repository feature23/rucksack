using FluentAssertions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

[Collection(TestCollections.StrategyTests)]
public class RepeatBurstLoadStrategyTests
{
    [Fact]
    public async Task Step_WithCountOf1_CallsActionOnce()
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new RepeatBurstLoadStrategy(1, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        LoadTask action = () =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.FromResult(new LoadTaskResult(TimeSpan.Zero));
        };
        var context = new LoadStrategyContext(PreviousResult: null, CurrentRunningTasks: 0);

        // Act
        var result = strategy.GenerateLoad(action, context);

        var tasks = await StrategyTestHelper.ExecuteStrategyResult(result);

        // Assert
        result.RepeatDelay.Should().Be(TimeSpan.FromSeconds(1));

        // Call again
        var currentRunningCount = tasks.Count(t => !t.IsCompleted);
        result = strategy.GenerateLoad(action, new LoadStrategyContext(PreviousResult: result, CurrentRunningTasks: currentRunningCount));

        tasks.AddRange(await StrategyTestHelper.ExecuteStrategyResult(result));
        await Task.WhenAll(tasks);

        // Assert
        actionCalledCount.Should().Be(1);
        result.RepeatDelay.Should().BeNull();
    }

    [Fact]
    public async Task Step_WithCountOf3_CallsActionThreeTimes()
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new RepeatBurstLoadStrategy(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        LoadTask action = () =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.FromResult(new LoadTaskResult(TimeSpan.Zero));
        };
        var context = new LoadStrategyContext(PreviousResult: null, CurrentRunningTasks: 0);

        // Act
        var result = strategy.GenerateLoad(action, context);

        var tasks = await StrategyTestHelper.ExecuteStrategyResult(result);

        // Assert
        result.RepeatDelay.Should().Be(TimeSpan.FromSeconds(1));

        // Call again
        var currentRunningCount = tasks.Count(t => !t.IsCompleted);
        result = strategy.GenerateLoad(action, new LoadStrategyContext(PreviousResult: result, CurrentRunningTasks: currentRunningCount));

        tasks.AddRange(await StrategyTestHelper.ExecuteStrategyResult(result));
        await Task.WhenAll(tasks);

        // Assert
        actionCalledCount.Should().Be(3);
        result.RepeatDelay.Should().BeNull();
    }
}
