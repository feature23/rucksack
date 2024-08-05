namespace Rucksack;

public record LoadStrategyResult(TimeSpan? NextStepDelay, IReadOnlyList<ValueTask<LoadTaskResult>>? Tasks)
{
    public static LoadStrategyResult Finished { get; } = new(null, null);
}
