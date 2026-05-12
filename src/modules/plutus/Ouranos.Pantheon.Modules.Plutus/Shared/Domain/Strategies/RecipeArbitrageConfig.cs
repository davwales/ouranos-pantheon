namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;

public sealed record RecipeArbitrageConfig(
    decimal? MinMarginPercent = null
);