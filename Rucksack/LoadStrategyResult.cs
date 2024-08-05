namespace Rucksack;

public record LoadStrategyResult(TimeSpan? NextStepDelay, IReadOnlyList<Task>? Tasks)
{
    public static LoadStrategyResult Finished { get; } = new(null, null);
}
