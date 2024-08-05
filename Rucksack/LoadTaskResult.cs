namespace Rucksack;

public readonly record struct LoadTaskResult(TimeSpan Duration, Exception? Exception = null)
{
    public bool IsSuccess => Exception is null;
}
