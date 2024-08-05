namespace Rucksack;

public interface ILoadStrategy
{
    LoadStrategyResult Step(Func<Task> action, LoadStrategyResult? previousResult);
}
