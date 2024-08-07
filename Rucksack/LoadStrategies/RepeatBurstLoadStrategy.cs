using System.Diagnostics;

namespace Rucksack.LoadStrategies;

public class RepeatLoadStrategy(int countPerInterval, TimeSpan interval, TimeSpan totalDuration)
    : ILoadStrategy
{
    public LoadStrategyResult GenerateLoad(LoadTask action, LoadStrategyContext context)
    {
        RepeatBurstLoadStrategyResult result;
        int iteration = 1;

        if (context.PreviousResult is null)
        {
            result = new RepeatBurstLoadStrategyResult(interval, Stopwatch.StartNew(), iteration, null);
        }
        else if (context.PreviousResult is not RepeatBurstLoadStrategyResult previousRepeatResult)
        {
            throw new ArgumentException($"Expected previous result type {nameof(RepeatBurstLoadStrategyResult)} but got {context.PreviousResult.GetType().Name}", nameof(context));
        }
        else
        {
            result = previousRepeatResult;
            iteration = result.Iteration + 1;
        }

        if (result.Stopwatch.Elapsed >= totalDuration)
        {
            return LoadStrategyResult.Finished;
        }

        var tasks = Enumerable.Repeat(action, countPerInterval).ToArray();

        return result with
        {
            Iteration = iteration,
            Tasks = tasks,
        };
    }

    private record RepeatBurstLoadStrategyResult(TimeSpan? RepeatDelay, Stopwatch Stopwatch, int Iteration, IReadOnlyList<LoadTask>? Tasks)
        : LoadStrategyResult(RepeatDelay, Tasks);
}
