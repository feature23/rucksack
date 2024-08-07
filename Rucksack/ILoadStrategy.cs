namespace Rucksack;

public interface ILoadStrategy
{
    LoadStrategyResult GenerateLoad(LoadTask action, LoadStrategyContext context);
}
