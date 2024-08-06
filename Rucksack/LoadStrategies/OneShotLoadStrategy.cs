namespace Rucksack.LoadStrategies;

public class OneShotLoadStrategy(int count) : ILoadStrategy
{
    public LoadStrategyResult GenerateLoad(Func<ValueTask<LoadTaskResult>> action, LoadStrategyContext context)
    {
        if (context.PreviousResult is not null)
        {
            throw new InvalidOperationException("OneShotLoadStrategy does not support previous results.");
        }

        var tasks = Enumerable.Range(0, count)
            .Select(_ => action())
            .ToArray();

        return new LoadStrategyResult(RepeatDelay: null, Tasks: tasks);
    }
}
