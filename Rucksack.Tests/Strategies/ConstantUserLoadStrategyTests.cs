using FluentAssertions;
using Rucksack.LoadStrategies;

namespace Rucksack.Tests.Strategies;

public class ConstantUserLoadStrategyTests
{
    [Fact]
    public async Task ConstantLoadStrategy_BasicTest()
    {
        // Arrange
        const int expectedCount = 46; // [10, 9, 9, 9, 9] since the first call runs for 5 seconds
        var actionCalledCount = 0;
        var strategy = new ConstantUserLoadStrategy(count: 10, checkInterval: TimeSpan.FromSeconds(1), totalDuration: TimeSpan.FromSeconds(5));
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
