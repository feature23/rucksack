namespace Rucksack;

public interface ILoadStrategy
{
    LoadStrategyResult GenerateLoad(Func<ValueTask<LoadTaskResult>> action, LoadStrategyContext context);
}
