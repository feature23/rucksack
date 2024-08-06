namespace Rucksack;

public record LoadStrategyResult(TimeSpan? RepeatDelay, IReadOnlyList<ValueTask<LoadTaskResult>>? Tasks)
{
    public static LoadStrategyResult Finished { get; } = new(null, null);
}
