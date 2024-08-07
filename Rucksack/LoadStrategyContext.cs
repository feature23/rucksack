namespace Rucksack;

public record LoadStrategyContext(LoadStrategyResult? PreviousResult, int CurrentRunningTasks);
