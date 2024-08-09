using FluentAssertions;

namespace Rucksack.Tests.Strategies;

public static class StrategyTestHelper
{
    public static readonly LoadTask NullTask = () => Task.FromResult(new LoadTaskResult(TimeSpan.Zero));

    public static async Task RunFullStrategyTest(ILoadStrategy strategy, LoadTask action)
    {
        LoadStrategyResult? result = null;
        List<Task<LoadTaskResult>> tasks = [];

        do
        {
            var currentRunningCount = tasks.Count(t => !t.IsCompleted);
            var context = new LoadStrategyContext(PreviousResult: result, CurrentRunningTasks: currentRunningCount);
            result = strategy.GenerateLoad(action, context);

            if (result.Tasks is { } resultTasks)
            {
                tasks.AddRange(resultTasks.Select(i => Task.Run(() => i())));
            }

            if (result.RepeatDelay.HasValue)
            {
                // NOTE: using Thread.Sleep instead of Task.Delay to avoid changing
                // threads/processors due to possible HAL bugs with getting accurate
                // data in a multithreaded environment for strategy Stopwatch use.
                // See: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch?view=net-8.0#remarks
                Thread.Sleep(result.RepeatDelay.Value);
            }
        }
        while (result.RepeatDelay.HasValue);

        await Task.WhenAll(tasks);

        result.RepeatDelay.Should().BeNull();
    }

    public static int ExecuteStrategyResult(LoadStrategyResult result)
    {
        if (result.RepeatDelay.HasValue)
        {
            // NOTE: using Thread.Sleep instead of Task.Delay to avoid changing
            // threads/processors due to possible HAL bugs with getting accurate
            // data in a multithreaded environment for strategy Stopwatch use.
            // See: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch?view=net-8.0#remarks
            Thread.Sleep(result.RepeatDelay.Value);
        }

        return result.Tasks?.Count ?? 0;
    }
}
