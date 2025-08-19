using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;

namespace Ouranos.Pantheon.Plutus.Service.Domain.Recipes;

public class Recipe : BaseEntity<Id<Recipe>>
{
    private Recipe(Id<Recipe> id) : base(id)
    {
        Name = string.Empty;
        Inputs = [];
        Outputs = [];
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; init; }

    public decimal Cost { get; init; }

    public IReadOnlyList<RecipeComponent> Inputs { get; init; }

    public IReadOnlyList<RecipeComponent> Outputs { get; init; }

    public virtual required Market Market { get; init; }

    public static Recipe Create(
        Id<Recipe> id,
        Market market,
        string name,
        decimal cost,
        IReadOnlyList<RecipeComponent> inputs,
        IReadOnlyList<RecipeComponent> outputs
    )
    {
        Guard.Against.Null(market);
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(inputs);
        Guard.Against.Null(outputs);

        return new Recipe(id)
        {
            MarketId = market.Id,
            Name = name,
            Cost = cost,
            Inputs = inputs,
            Outputs = outputs,
            Market = market
        };
    }
}