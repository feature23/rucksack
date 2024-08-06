namespace Rucksack;

public interface ILoadStrategy
{
    LoadStrategyResult Step(Func<ValueTask<LoadTaskResult>> action, LoadStrategyContext context);
}
