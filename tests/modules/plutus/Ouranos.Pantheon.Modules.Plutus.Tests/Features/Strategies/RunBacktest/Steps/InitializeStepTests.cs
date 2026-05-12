using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class InitializeStepTests
{
    private readonly IFixture _fixture = new Fixture();

    public InitializeStepTests()
    {
        _fixture.Customize(new IdCustomization());
    }

    [Theory]
    [InlineData(1, 1)]
    [InlineData(15, 1)]
    [InlineData(30, 1)]
    [InlineData(31, 3)]
    [InlineData(60, 3)]
    [InlineData(90, 3)]
    [InlineData(91, 7)]
    [InlineData(180, 7)]
    [InlineData(365, 7)]
    [InlineData(366, 14)]
    [InlineData(730, 14)]
    public void DetermineWindowSize_WhenGivenTotalDays_ReturnsCorrectWindow(
        int totalDays,
        int expectedWindow
    )
    {
        // Arrange - handled by InlineData

        // Act
        var result = InitializeStep.DetermineWindowSize(totalDays);

        // Assert
        result.ShouldBe(expectedWindow);
    }

    [Fact]
    public void DetermineWindowSize_WhenZeroDays_ReturnsOneDayWindow()
    {
        // Arrange
        const int totalDays = 0;

        // Act
        var result = InitializeStep.DetermineWindowSize(totalDays);

        // Assert
        result.ShouldBe(1);
    }

    [Fact]
    public void GetTaxRate_WhenMarketHasFlatTax_ReturnsRate()
    {
        // Arrange
        var market = Market.Create(
            _fixture.Create<Id<Market>>(),
            "Test Market",
            new Taxes(new FlatTax(0, 500, 0.10m))
        );

        // Act
        var result = InitializeStep.GetTaxRate(market);

        // Assert
        result.ShouldBe(0.10m);
    }

    [Fact]
    public void GetTaxRate_WhenMarketHasNoFlatTax_ReturnsZero()
    {
        // Arrange
        var market = Market.Create(_fixture.Create<Id<Market>>(), "Test Market", new Taxes(null));

        // Act
        var result = InitializeStep.GetTaxRate(market);

        // Assert
        result.ShouldBe(0m);
    }
}
