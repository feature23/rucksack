# Rucksack

A simple load testing library for .NET.

## Getting Started

Currently, you must build the project from source and add as a reference manually. A NuGet release will come soon.

```c#
using Rucksack;
using Rucksack.LoadStrategies;

var options = new LoadTestOptions
{
    LoadStrategy = new RepeatLoadStrategy(
        CountPerStep: 10,
        Interval: TimeSpan.FromSeconds(1),
        TotalDuration: TimeSpan.FromSeconds(10)),
};

await LoadTestRunner.Run(async () =>
{
    // do something to put load on the system...
    await Task.Delay(1000);
}, options);
```

## Load Strategies

There are a couple built-in strategies for generating load, or you can implement `ILoadStrategy` yourself for custom logic.

* `OneShotLoadStrategy`: Enqueue the given number of tasks all at once. Does not repeat.
* `RepeatLoadStrategy`: Repeat enqueueing the given count of tasks at each given interval until the given duration has passed.
