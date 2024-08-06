namespace Rucksack.LoadStrategies;

public class SteppedBurstLoadStrategy : ILoadStrategy
{
    private readonly int _step;
    private readonly int _from;
    private readonly int _to;
    private readonly TimeSpan _interval;

    public SteppedBurstLoadStrategy(int step, int from, int to, TimeSpan interval)
    {
        if (from == to)
        {
            throw new ArgumentException(
                "From and to values must be different. " +
                $"If you just need to fire a burst once, consider using {nameof(OneShotLoadStrategy)}.",
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

        _step = step;
        _from = from;
        _to = to;
        _interval = interval;
    }

    public LoadStrategyResult GenerateLoad(Func<ValueTask<LoadTaskResult>> action, LoadStrategyContext context)
    {
        SteppedLoadStrategyResult result;
        int currentCount = _from;

        if (context.PreviousResult is null)
        {
            result = new SteppedLoadStrategyResult(_interval, currentCount, null);
        }
        else if (context.PreviousResult is not SteppedLoadStrategyResult previousSteppedResult)
        {
            throw new ArgumentException($"Expected previous result type {nameof(SteppedLoadStrategyResult)} but got {context.PreviousResult.GetType().Name}", nameof(context));
        }
        else
        {
            result = previousSteppedResult;
            currentCount = result.CurrentCount + _step;
        }

        var tasks = Enumerable.Range(0, currentCount)
            .Select(_ => action())
            .ToArray();

        return result with
        {
            RepeatDelay = IsFinished(_from, _to, currentCount) ? null : _interval,
            CurrentCount = currentCount,
            Tasks = tasks,
        };
    }

    internal static bool IsFinished(int from, int to, int currentCount) =>
        (from < to && currentCount >= to)
        || (from > to && currentCount <= to);

    private record SteppedLoadStrategyResult(TimeSpan? RepeatDelay, int CurrentCount, IReadOnlyList<ValueTask<LoadTaskResult>>? Tasks)
        : LoadStrategyResult(RepeatDelay, Tasks);
}
