using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Shared;

public sealed class PlutusOptionsTests
{
    [Fact]
    public void PlutusOptions_DefaultConstructor_ShouldSetDefaults()
    {
        // Act
        var options = new PlutusOptions();

        // Assert
        options.DataLoaders.ShouldNotBeNull();
        options.Forecasting.ShouldNotBeNull();
        options.SymbolAnalyst.ShouldNotBeNull();
        options.MarketAnalyst.ShouldNotBeNull();
        PlutusOptions.SectionName.ShouldBe("Ouranos:Plutus");
    }

    [Fact]
    public void SymbolAnalyst_ShouldDefaultToGlobalModelDefaults()
    {
        // Act
        var options = new PlutusOptions().SymbolAnalyst;

        // Assert
        options.ModelName.ShouldBe(ModelDefaults.ModelName);
        options.MaxTokens.ShouldBe(ModelDefaults.MaxTokens);
        options.ReasoningEffort.ShouldBe(ModelDefaults.ReasoningEffort);
    }

    [Fact]
    public void MarketAnalyst_ShouldDefaultToGlobalModelDefaults()
    {
        // Act
        var options = new PlutusOptions().MarketAnalyst;

        // Assert
        options.ModelName.ShouldBe(ModelDefaults.ModelName);
        options.MaxTokens.ShouldBe(1536);
        options.ReasoningEffort.ShouldBe(ModelDefaults.ReasoningEffort);
        options.MaxViewRows.ShouldBe(12);
    }
}
