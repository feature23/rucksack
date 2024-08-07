namespace Rucksack;

public record LoadStrategyResult(TimeSpan? RepeatDelay, IReadOnlyList<LoadTask>? Tasks)
{
    public static LoadStrategyResult Finished { get; } = new(null, null);
}
