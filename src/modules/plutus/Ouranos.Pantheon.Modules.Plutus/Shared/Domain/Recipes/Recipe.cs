using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

public class Recipe : BaseEntity<Id<Recipe>>
{
    protected Recipe(Id<Recipe> id) : base(id)
    {
        Name = string.Empty;
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; init; }

    public decimal Cost { get; init; }

    public virtual required ICollection<RecipeComponent> Inputs { get; init; }

    public virtual required ICollection<RecipeComponent> Outputs { get; init; }

    public virtual required Market Market { get; init; }

    public static Recipe Create(
        Id<Recipe> id,
        Market market,
        string name,
        decimal cost,
        ICollection<RecipeComponent> inputs,
        ICollection<RecipeComponent> outputs
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