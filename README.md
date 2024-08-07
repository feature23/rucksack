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
        countPerInterval: 10,
        interval: TimeSpan.FromSeconds(1),
        totalDuration: TimeSpan.FromSeconds(10)),
};

await LoadTestRunner.Run(async () =>
{
    // do something to put load on the system...
    await Task.Delay(1000);
}, options);
```

## Load Strategies

There are a couple built-in strategies for generating load, or you can implement `ILoadStrategy` yourself for custom logic.

* `ConstantUserLoadStrategy`: Attempts to maintain a constant concurrent user load at each given check interval until the given duration has passed.
* `OneShotBurstLoadStrategy`: Enqueue the given number of tasks all at once. Does not repeat.
* `RepeatBurstLoadStrategy`: Repeat enqueueing the given count of tasks at each given interval until the given duration has passed.
* `SteppedBurstLoadStrategy`: Enqueue an increasing (or decreasing) burst of tasks in a stepwise manner, regardless of how many are still running.
* `SteppedUserLoadStrategy`: Attempts to maintain an increasing (or decreasing) concurrent user load in a stepwise manner.
