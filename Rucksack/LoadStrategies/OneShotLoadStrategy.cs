namespace Rucksack.LoadStrategies;

public class OneShotLoadStrategy(int count) : ILoadStrategy
{
    public int Count { get; } = count;

    public LoadStrategyResult Step(Func<Task> action, LoadStrategyResult? previousResult)
    {
        if (previousResult is not null)
        {
            throw new InvalidOperationException("OneShotLoadStrategy does not support previous results.");
        }

        var tasks = Enumerable.Range(0, Count).Select(_ => action()).ToArray();

        return new LoadStrategyResult(NextStepDelay: null, Tasks: tasks);
    }
}
