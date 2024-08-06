using System.Diagnostics;

namespace Rucksack.LoadStrategies;

public class RepeatLoadStrategy(int CountPerStep, TimeSpan Interval, TimeSpan TotalDuration)
    : ILoadStrategy
{
    public LoadStrategyResult Step(Func<ValueTask<LoadTaskResult>> action, LoadStrategyContext context)
    {
        RepeatLoadStrategyResult result;
        int iteration = 1;

        if (context.PreviousResult is null)
        {
            result = new RepeatLoadStrategyResult(Interval, Stopwatch.StartNew(), iteration, null);
        }
        else if (context.PreviousResult is not RepeatLoadStrategyResult previousRepeatResult)
        {
            throw new ArgumentException($"Expected previous result type {nameof(RepeatLoadStrategyResult)} but got {context.PreviousResult.GetType().Name}", nameof(context));
        }
        else
        {
            result = previousRepeatResult;
            iteration = result.Iteration + 1;
        }

        if (result.Stopwatch.Elapsed >= TotalDuration)
        {
            return LoadStrategyResult.Finished;
        }

        var tasks = Enumerable.Range(0, CountPerStep)
            .Select(_ => action())
            .ToArray();

        return result with
        {
            Iteration = iteration,
            Tasks = tasks,
        };
    }

    private record RepeatLoadStrategyResult(TimeSpan? NextStepDelay, Stopwatch Stopwatch, int Iteration, IReadOnlyList<ValueTask<LoadTaskResult>>? Tasks)
        : LoadStrategyResult(NextStepDelay, Tasks);
}
