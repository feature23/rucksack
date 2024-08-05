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
            builder.AddConsole();
        });

        var logger = loggerFactory.CreateLogger(nameof(LoadTestRunner));

        logger.LogInformation("Rucksack is running...");

        LoadStrategyResult? result = null;
        List<Task> allTasks = [];

        while (true)
        {
            result = options.LoadStrategy.Step(action, result);

            if (result.Tasks is { } tasks)
            {
                logger.LogInformation("Enqueueing {Count} new tasks", tasks.Count);

                allTasks.AddRange(tasks);
            }

            if (result.NextStepDelay == null)
            {
                break;
            }

            Thread.Sleep(result.NextStepDelay.Value);
        }

        logger.LogInformation("Waiting for {Count} tasks to complete...", allTasks.Count);

        await Task.WhenAll(allTasks);

        logger.LogInformation("Rucksack has finished");
    }
}
