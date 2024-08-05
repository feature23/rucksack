using System.Diagnostics;

namespace Rucksack.LoadStrategies;

public class RepeatLoadStrategy(int CountPerStep, TimeSpan Interval, TimeSpan TotalDuration)
    : ILoadStrategy
{
    public LoadStrategyResult Step(Func<ValueTask<LoadTaskResult>> action, LoadStrategyResult? previousResult)
    {
        RepeatLoadStrategyResult result;
        int iteration = 1;

        if (previousResult is null)
        {
            result = new RepeatLoadStrategyResult(Interval, Stopwatch.StartNew(), iteration, null);
        }
        else if (previousResult is not RepeatLoadStrategyResult previousRepeatResult)
        {
            throw new ArgumentException($"Expected {nameof(RepeatLoadStrategyResult)} but got {previousResult.GetType().Name}", nameof(previousResult));
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
