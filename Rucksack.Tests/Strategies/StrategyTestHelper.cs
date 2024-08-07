using FluentAssertions;

namespace Rucksack.Tests.Strategies;

public static class StrategyTestHelper
{
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
                await Task.Delay(result.RepeatDelay.Value);
            }
        }
        while (result.RepeatDelay.HasValue);

        await Task.WhenAll(tasks);

        result.RepeatDelay.Should().BeNull();
    }

    public static async Task ExecuteStrategyResultAndWait(LoadStrategyResult result)
    {
        var tasks = await ExecuteStrategyResult(result);

        await Task.WhenAll(tasks);
    }

    public static async Task<List<Task<LoadTaskResult>>> ExecuteStrategyResult(LoadStrategyResult result)
    {
        var tasks = result.Tasks?.Select(i => Task.Run(() => i())).ToList() ?? [];

        if (result.RepeatDelay.HasValue)
        {
            await Task.Delay(result.RepeatDelay.Value);
        }

        return tasks;
    }
}
