namespace Rucksack.Tests.Strategies;

public static class StrategyTestHelper
{
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
