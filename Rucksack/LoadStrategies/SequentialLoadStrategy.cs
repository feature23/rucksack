using System.Collections;

namespace Rucksack.LoadStrategies;

public class SequentialLoadStrategy(TimeSpan intervalBetweenStrategies)
    : ILoadStrategy, IEnumerable<ILoadStrategy>
{
    private readonly List<ILoadStrategy> _loadStrategies = [];

    public void Add(ILoadStrategy loadStrategy) => _loadStrategies.Add(loadStrategy);

    public LoadStrategyResult GenerateLoad(LoadTask action, LoadStrategyContext context)
    {
        if (_loadStrategies.Count == 0)
        {
            throw new InvalidOperationException("No load strategies added to the sequential load strategy.");
        }

        SequentialLoadStrategyResult result;
        LoadStrategyResult? innerStrategyResult = null;
        int strategyIndex = 0;

        if (context.PreviousResult is null)
        {
            result = new SequentialLoadStrategyResult(null, strategyIndex, null, null);
        }
        else if (context.PreviousResult is not SequentialLoadStrategyResult previousSequentialResult)
        {
            throw new ArgumentException($"Expected previous result type {nameof(SequentialLoadStrategyResult)} but got {context.PreviousResult.GetType().Name}", nameof(context));
        }
        else
        {
            result = previousSequentialResult;

            if (result.StrategyResult is not null)
            {
                innerStrategyResult = result.StrategyResult;
                strategyIndex = result.StrategyIndex;
            }
            else
            {
                strategyIndex = result.StrategyIndex + 1;
            }
        }

        if (strategyIndex >= _loadStrategies.Count)
        {
            return LoadStrategyResult.Finished;
        }

        var currentStrategy = _loadStrategies[strategyIndex];

        var currentResult = currentStrategy.GenerateLoad(action, context with
        {
            PreviousResult = innerStrategyResult,
        });

        return result with
        {
            RepeatDelay = currentResult.RepeatDelay ?? intervalBetweenStrategies,
            StrategyIndex = strategyIndex,
            Tasks = currentResult.Tasks,
            StrategyResult = currentResult.RepeatDelay == null ? null : currentResult,
        };
    }

    private record SequentialLoadStrategyResult(TimeSpan? RepeatDelay, int StrategyIndex, LoadStrategyResult? StrategyResult, IReadOnlyList<LoadTask>? Tasks)
        : LoadStrategyResult(RepeatDelay, Tasks);

    public IEnumerator<ILoadStrategy> GetEnumerator() => _loadStrategies.GetEnumerator();

    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}
