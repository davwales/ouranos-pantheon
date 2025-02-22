using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;

namespace Ouranos.Pantheon.Service.Plutus.Domain.Recipes;

public sealed class Recipe : BaseEntity<Id<Recipe>>
{
    public Recipe(
        Id<Recipe> id,
        Id<Market> marketId,
        string name,
        decimal cost,
        IReadOnlyList<RecipeComponent> inputs,
        IReadOnlyList<RecipeComponent> outputs
    ) : base(id)
    {
        Guard.Against.Null(marketId);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(inputs);
        Guard.Against.NullOrEmpty(outputs);

        MarketId = marketId;
        Name = name;
        Cost = cost;
        Inputs = inputs;
        Outputs = outputs;
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; init; }

    public decimal Cost { get; init; }

    public IReadOnlyList<RecipeComponent> Inputs { get; init; }

    public IReadOnlyList<RecipeComponent> Outputs { get; init; }
}