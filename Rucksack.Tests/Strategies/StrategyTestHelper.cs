namespace Rucksack.Tests.Strategies;

public static class StrategyTestHelper
{
    public static async ValueTask ExecuteStrategyResultAndWait(LoadStrategyResult result)
    {
        var tasks = await ExecuteStrategyResult(result);

        await WhenAll(tasks);
    }

    public static async ValueTask WhenAll(IEnumerable<ValueTask<LoadTaskResult>> tasks)
    {
        foreach (var task in tasks)
        {
            await task;
        }
    }

    public static async ValueTask<List<ValueTask<LoadTaskResult>>> ExecuteStrategyResult(LoadStrategyResult result)
    {
        if (result.NextStepDelay.HasValue)
        {
            await Task.Delay(result.NextStepDelay.Value);
        }

        return [..result.Tasks ?? []];
    }
}
