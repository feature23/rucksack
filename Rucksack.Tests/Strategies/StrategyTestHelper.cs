namespace Rucksack.Tests.Strategies;

public static class StrategyHelper
{
    public static async Task ExecuteStrategyResultAndWait(LoadStrategyResult result)
        => await Task.WhenAll(await ExecuteStrategyResult(result));

    public static async Task<List<Task>> ExecuteStrategyResult(LoadStrategyResult result)
    {
        if (result.NextStepDelay.HasValue)
        {
            await Task.Delay(result.NextStepDelay.Value);
        }

        return [..result.Tasks ?? []];
    }
}
