using FluentAssertions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

[Collection(TestCollections.StrategyTests)]
public class SteppedUserLoadStrategyTests
{
    [Fact]
    public async Task SteppedUserLoadStrategy_BasicTest()
    {
        // Arrange
        const int expectedCount = 146; // [10, 19, 29, 39, 49] since the first call runs for 5 seconds
        var actionCalledCount = 0;
        var strategy = new SteppedUserLoadStrategy(
            step: 10,
            from: 10,
            to: 50,
            checkInterval: TimeSpan.FromSeconds(1),
            stepInterval: TimeSpan.FromSeconds(1),
            totalDuration: TimeSpan.FromSeconds(5));

        LoadTask action = async () =>
        {
            int newCount = Interlocked.Increment(ref actionCalledCount);

            if (newCount == 1)
            {
                await Task.Delay(5000);
            }

            return new LoadTaskResult(TimeSpan.Zero);
        };

        // Act
        await StrategyTestHelper.RunFullStrategyTest(strategy, action);

        // Assert
        actionCalledCount.Should().Be(expectedCount);
    }
}
