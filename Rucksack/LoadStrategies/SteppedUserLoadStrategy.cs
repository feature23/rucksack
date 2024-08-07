using System.Diagnostics;

namespace Rucksack.LoadStrategies;

public class SteppedUserLoadStrategy : ILoadStrategy
{
    private readonly int _step;
    private readonly int _from;
    private readonly int _to;
    private readonly TimeSpan _stepInterval;
    private readonly TimeSpan _checkInterval;
    private readonly TimeSpan _totalDuration;

    public SteppedUserLoadStrategy(int step,
        int from,
        int to,
        TimeSpan stepInterval,
        TimeSpan checkInterval,
        TimeSpan totalDuration)
    {
        if (from == to)
        {
            throw new ArgumentException(
                "From and to values must be different. " +
                $"If you just need to maintain a constant user load, consider using {nameof(ConstantUserLoadStrategy)}.",
                nameof(from));
        }

        if (from < to && step <= 0)
        {
            throw new ArgumentException("Step must be greater than 0 when from is less than to.", nameof(step));
        }

        if (from > to && step >= 0)
        {
            throw new ArgumentException("Step must be less than 0 when from is greater than to.", nameof(step));
        }

        if (stepInterval < checkInterval)
        {
            throw new ArgumentException("Step interval must be greater than or equal to check interval.", nameof(stepInterval));
        }

        if (totalDuration < stepInterval)
        {
            throw new ArgumentException("Total duration must be greater than or equal to step interval.", nameof(totalDuration));
        }

        _step = step;
        _from = from;
        _to = to;
        _stepInterval = stepInterval;
        _checkInterval = checkInterval;
        _totalDuration = totalDuration;
    }

    public LoadStrategyResult GenerateLoad(LoadTask action, LoadStrategyContext context)
    {
        SteppedUserLoadStrategyResult result;
        int currentCount = _from;

        if (context.PreviousResult is null)
        {
            result = new SteppedUserLoadStrategyResult(
                RepeatDelay: _checkInterval,
                CurrentCount: currentCount,
                StepStopwatch: Stopwatch.StartNew(),
                TotalStopwatch: Stopwatch.StartNew(),
                Tasks: null);
        }
        else if (context.PreviousResult is not SteppedUserLoadStrategyResult previousSteppedResult)
        {
            throw new ArgumentException($"Expected previous result type {nameof(SteppedUserLoadStrategyResult)} but got {context.PreviousResult.GetType().Name}", nameof(context));
        }
        else
        {
            result = previousSteppedResult;

            if (result.StepStopwatch.Elapsed >= _stepInterval
                && !IsFinished(_from, _to, result.CurrentCount))
            {
                currentCount = result.CurrentCount + _step;
                result = result with
                {
                    StepStopwatch = Stopwatch.StartNew(),
                };
            }
        }

        if (result.TotalStopwatch.Elapsed >= _totalDuration)
        {
            return LoadStrategyResult.Finished;
        }

        var tasks = Enumerable.Repeat(action, currentCount - context.CurrentRunningTasks).ToArray();

        return result with
        {
            RepeatDelay = _checkInterval,
            CurrentCount = currentCount,
            Tasks = tasks,
        };
    }

    private static bool IsFinished(int from, int to, int currentCount)
        => from < to ? currentCount >= to : currentCount <= to;

    private record SteppedUserLoadStrategyResult(TimeSpan? RepeatDelay,
        int CurrentCount,
        Stopwatch StepStopwatch,
        Stopwatch TotalStopwatch,
        IReadOnlyList<LoadTask>? Tasks)
        : LoadStrategyResult(RepeatDelay, Tasks);
}
