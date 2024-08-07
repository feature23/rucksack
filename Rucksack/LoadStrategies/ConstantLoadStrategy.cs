using System.Diagnostics;

namespace Rucksack.LoadStrategies;

public class ConstantLoadStrategy(int count, TimeSpan checkInterval, TimeSpan totalDuration)
    : ILoadStrategy
{
    public LoadStrategyResult GenerateLoad(LoadTask action, LoadStrategyContext context)
    {
        ConstantLoadStrategyResult result;

        if (context.PreviousResult is null)
        {
            result = new ConstantLoadStrategyResult(checkInterval, Stopwatch.StartNew(), null);
        }
        else if (context.PreviousResult is not ConstantLoadStrategyResult previousResult)
        {
            throw new ArgumentException($"Expected previous result type {nameof(ConstantLoadStrategyResult)} but got {context.PreviousResult.GetType().Name}", nameof(context));
        }
        else
        {
            result = previousResult;
        }

        if (result.Stopwatch.Elapsed >= totalDuration)
        {
            return LoadStrategyResult.Finished;
        }

        int countToSpawn = Math.Max(count - context.CurrentRunningTasks, 0);

        var tasks = Enumerable.Repeat(action, countToSpawn).ToArray();

        return result with
        {
            RepeatDelay = checkInterval,
            Tasks = tasks,
        };
    }

    private record ConstantLoadStrategyResult(TimeSpan? RepeatDelay, Stopwatch Stopwatch, IReadOnlyList<LoadTask>? Tasks)
        : LoadStrategyResult(RepeatDelay, Tasks);
}
