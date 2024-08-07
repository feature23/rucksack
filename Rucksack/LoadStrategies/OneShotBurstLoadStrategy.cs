namespace Rucksack.LoadStrategies;

public class OneShotLoadStrategy(int count) : ILoadStrategy
{
    public LoadStrategyResult GenerateLoad(LoadTask action, LoadStrategyContext context)
    {
        if (context.PreviousResult is not null)
        {
            throw new InvalidOperationException("OneShotLoadStrategy does not support previous results.");
        }

        var tasks = Enumerable.Repeat(action, count).ToArray();

        return new LoadStrategyResult(RepeatDelay: null, Tasks: tasks);
    }
}
