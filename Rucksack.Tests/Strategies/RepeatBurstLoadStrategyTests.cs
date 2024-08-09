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
        var strategy = new RepeatBurstLoadStrategy(1, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        var context = new LoadStrategyContext(PreviousResult: null, CurrentRunningTasks: 0);

        // Act
        var result = strategy.GenerateLoad(StrategyTestHelper.NullTask, context);

        var taskCount = await StrategyTestHelper.ExecuteStrategyResult(result);

        // Assert
        taskCount.Should().Be(1);

        // Call again
        result = strategy.GenerateLoad(StrategyTestHelper.NullTask,
            new LoadStrategyContext(PreviousResult: result, CurrentRunningTasks: 0));

        taskCount = await StrategyTestHelper.ExecuteStrategyResult(result);

        // Assert
        taskCount.Should().Be(0);
        result.RepeatDelay.Should().BeNull();
    }

    [Fact]
    public async Task Step_WithCountOf3_CallsActionThreeTimes()
    {
        // Arrange
        var strategy = new RepeatBurstLoadStrategy(3, TimeSpan.FromSeconds(1), TimeSpan.FromSeconds(1));
        var context = new LoadStrategyContext(PreviousResult: null, CurrentRunningTasks: 0);

        // Act
        var result = strategy.GenerateLoad(StrategyTestHelper.NullTask, context);

        var taskCount = await StrategyTestHelper.ExecuteStrategyResult(result);

        // Assert
        taskCount.Should().Be(3);
        result.RepeatDelay.Should().Be(TimeSpan.FromSeconds(1));

        // Call again
        result = strategy.GenerateLoad(StrategyTestHelper.NullTask,
            new LoadStrategyContext(PreviousResult: result, CurrentRunningTasks: 0));

        taskCount = await StrategyTestHelper.ExecuteStrategyResult(result);

        // Assert
        taskCount.Should().Be(0);
        result.RepeatDelay.Should().BeNull();
    }
}
