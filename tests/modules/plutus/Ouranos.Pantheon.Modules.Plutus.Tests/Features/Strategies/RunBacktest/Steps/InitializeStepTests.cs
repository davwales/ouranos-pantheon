using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest.Steps;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.RunBacktest.Steps;

public sealed class InitializeStepTests
{
    private readonly IFixture _fixture = new Fixture();

    public InitializeStepTests()
    {
        _fixture.Customize(new IdCustomization());
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
