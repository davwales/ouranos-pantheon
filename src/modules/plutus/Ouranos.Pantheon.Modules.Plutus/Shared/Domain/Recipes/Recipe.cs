using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;

public class Recipe : BaseEntity<Id<Recipe>>
{
    protected Recipe(Id<Recipe> id)
        : base(id)
    {
        Name = string.Empty;
    }

    public Id<Market> MarketId { get; init; }

    public string Name { get; init; }

    public decimal Cost { get; init; }

    private ICollection<RecipeComponent>? _inputs;

    public ICollection<RecipeComponent> Inputs =>
        _inputs ?? throw new NavigationPropertyNotLoadedException<Recipe>();

    private ICollection<RecipeComponent>? _outputs;

    public ICollection<RecipeComponent> Outputs =>
        _outputs ?? throw new NavigationPropertyNotLoadedException<Recipe>();

    private Market? _market;
    public Market Market => _market ?? throw new NavigationPropertyNotLoadedException<Recipe>();

    public static Recipe Create(
        Id<Recipe> id,
        Id<Market> marketId,
        string name,
        decimal cost,
        ICollection<RecipeComponent> inputs,
        ICollection<RecipeComponent> outputs,
        Market? market = null
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(inputs);
        Guard.Against.Null(outputs);

        if (market is not null)
        {
            Guard.Against.InvalidInput(market, nameof(market), m => m.Id == marketId);
        }

        return new Recipe(id)
        {
            MarketId = marketId,
            Name = name,
            Cost = cost,
            _inputs = inputs,
            _outputs = outputs,
            _market = market,
        };
    }
}
