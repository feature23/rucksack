
using Rucksack;
using Rucksack.LoadStrategies;

int completedTasks = 0;

await LoadTestRunner.Run(() =>
{
    var index = Interlocked.Increment(ref completedTasks);

    return index switch
    {
        _ when index % 3 == 0 => Task.FromException(new ArgumentOutOfRangeException("arg", "Bad argument!")),
        _ when index % 5 == 0 => Task.FromException(new InvalidOperationException("Invalid operation!")),
        _ => Task.Delay(TimeSpan.FromSeconds(1.1))
    };
}, new LoadTestOptions
{
    LoadStrategy = new SequentialLoadStrategy(TimeSpan.FromSeconds(1))
    {
        new OneShotLoadStrategy(10),
        new OneShotLoadStrategy(20),
        new OneShotLoadStrategy(15),
        new OneShotLoadStrategy(25),
        new OneShotLoadStrategy(30)
    }
});
