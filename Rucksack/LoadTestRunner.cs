using System.Diagnostics;
using Microsoft.Extensions.Logging;

namespace Rucksack;

public static class LoadTestRunner
{
    public static void Run(Action action, LoadTestOptions options) =>
        Run(() =>
        {
            action();
            return Task.CompletedTask;
        }, options).Wait();

    public static async Task Run(Func<Task> action, LoadTestOptions options)
    {
        var loggerFactory = options.LoggerFactory ?? LoggerFactory.Create(builder =>
        {
            builder.AddConsole().SetMinimumLevel(LogLevel.Information);
        });

        var logger = loggerFactory.CreateLogger(nameof(LoadTestRunner));

        logger.LogInformation("Rucksack is running...");

        LoadStrategyResult? result = null;
        List<Task<LoadTaskResult>> allTasks = [];

        while (true)
        {
            var runningCount = allTasks.Count(t => !t.IsCompleted);

            var context = new LoadStrategyContext(PreviousResult: result, CurrentRunningTasks: runningCount);

            result = options.LoadStrategy.GenerateLoad(LoadAction, context);

            if (result.Tasks is { } tasks)
            {
                logger.LogDebug("Enqueueing {Count} new tasks", tasks.Count);

                allTasks.AddRange(tasks.Select(task => Task.Run(() => task())));
            }

            if (result.RepeatDelay == null)
            {
                break;
            }

            // NOTE: using Thread.Sleep instead of Task.Delay to avoid changing
            // threads/processors due to possible HAL bugs with getting accurate
            // data in a multithreaded environment for strategy Stopwatch use.
            // See: https://learn.microsoft.com/en-us/dotnet/api/system.diagnostics.stopwatch?view=net-8.0#remarks
            Thread.Sleep(result.RepeatDelay.Value);
        }

        logger.LogInformation("Waiting for {Count} tasks to complete...", allTasks.Count);

        List<LoadTaskResult> results = new(allTasks.Count);

        foreach (var task in allTasks)
        {
            results.Add(await task);
        }

        var successCount = results.Count(r => r.IsSuccess);
        var totalCount = results.Count;
        var passRate = successCount / (double)totalCount;
        logger.LogInformation("Passed: {SuccessCount}/{TotalCount} ({PassRate:P1})", successCount, totalCount, passRate);

        if (successCount != totalCount)
        {
            var exceptionCountsByType = results
                .Where(r => r.Exception is not null)
                .GroupBy(r => r.Exception!.GetType())
                .Select(g => (Type: g.Key, Count: g.Count()))
                .OrderByDescending(g => g.Count);

            logger.LogInformation("Exceptions by type:");

            foreach (var (exceptionType, count) in exceptionCountsByType)
            {
                logger.LogInformation("{ExceptionType}: {Count}", exceptionType.Name, count);
            }
        }

        var avgDuration = TimeSpan.FromMilliseconds(results
            .Select(r => r.Duration.TotalMilliseconds)
            .Average());

        logger.LogInformation("Average duration: {AvgDuration}", avgDuration);

        logger.LogInformation("Rucksack has finished");

        return;

        async Task<LoadTaskResult> LoadAction()
        {
            var stopwatch = Stopwatch.StartNew();

            try
            {
                await action();
                stopwatch.Stop();
                return new LoadTaskResult(stopwatch.Elapsed);
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                return new LoadTaskResult(stopwatch.Elapsed, ex);
            }
        }
    }
}
