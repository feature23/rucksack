using FluentAssertions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

public class SteppedBurstLoadStrategyTests
{
    [InlineData(1, 1, 2, 3)] // generated load = [1, 2] for a total of 3
    [InlineData(2, 2, 4, 6)] // generated load = [2, 4] for a total of 6
    [InlineData(10, 10, 50, 150)] // generated load = [10, 20, 30, 40, 50] for a total of 150
    [InlineData(-10, 50, 10, 150)] // generated load = [50, 40, 30, 20, 10] for a total of 150
    [InlineData(-1, 2, 1, 3)] // generated load = [2, 1] for a total of 3
    [Theory]
    public async Task SteppedBurstLoadStrategy_FullTests(int step, int from, int to, int expectedCount)
    {
        // Arrange
        var actionCalledCount = 0;
        var strategy = new SteppedBurstLoadStrategy(step, from, to, interval: TimeSpan.FromSeconds(1));
        LoadTask action = () =>
        {
            Interlocked.Increment(ref actionCalledCount);
            return Task.FromResult(new LoadTaskResult(TimeSpan.Zero));
        };

        // Act
        LoadStrategyResult? result = null;
        List<Task<LoadTaskResult>> tasks = [];

        do
        {
            var context = new LoadStrategyContext(PreviousResult: result);
            result = strategy.GenerateLoad(action, context);

            if (result.Tasks is { } resultTasks)
            {
                tasks.AddRange(resultTasks.Select(i => Task.Run(() => i())));
            }

            if (result.RepeatDelay.HasValue)
            {
                await Task.Delay(result.RepeatDelay.Value);
            }
        }
        while (result.RepeatDelay.HasValue);

        await Task.WhenAll(tasks);

        // Assert
        result.RepeatDelay.Should().BeNull();
        actionCalledCount.Should().Be(expectedCount);
    }

    [InlineData(0, 1, 0, false)]
    [InlineData(0, 1, 1, true)]
    [InlineData(0, 1, 2, true)]
    [InlineData(1, 0, 1, false)]
    [InlineData(1, 0, 0, true)]
    [InlineData(1, 0, -1, true)]
    [InlineData(10, 100, 99, false)]
    [InlineData(10, 100, 101, true)]
    [InlineData(100, 10, 11, false)]
    [InlineData(100, 10, 8, true)]
    [Theory]
    public void IsFinishedTests(int from, int to, int current, bool expected)
    {
        // Act
        var result = SteppedBurstLoadStrategy.IsFinished(from, to, current);

        // Assert
        result.Should().Be(expected);
    }
}
