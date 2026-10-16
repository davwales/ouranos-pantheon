using Marten;
using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.AskKitchenAssistant.Schemas;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Tests.Features.Assistants.AskKitchenAssistant;

/// <summary>
/// A two-step, two-ingredient recipe behind a stubbed store, shared by the kitchen tool tests.
/// </summary>
internal sealed class KitchenRecipeFixture
{
    public static readonly Guid RecipeId = Guid.NewGuid();

    public static readonly KitchenAssistantContext Context = new(
        new Id<Recipe>(RecipeId.ToString())
    );

    public KitchenRecipeFixture()
    {
        Store.QuerySession().Returns(Session);
        Returns(BuildRecipe());
    }

    public IHestiaMartenStore Store { get; } = Substitute.For<IHestiaMartenStore>();

    public IQuerySession Session { get; } = Substitute.For<IQuerySession>();

    public static Recipe BuildRecipe(string notes = "Best served warm.")
    {
        return Recipe
            .Create(
                RecipeId,
                "Chocolate Cake",
                null,
                [new Step("Mix the batter."), new Step("Bake for 30 minutes.")],
                [new Ingredient(1.5m, "cups", "flour"), new Ingredient(2m, "tbsp", "parsley")],
                notes
            )
            .State;
    }

    public void Returns(Recipe recipe)
    {
        Session.LoadAsync<Recipe>(RecipeId, Arg.Any<CancellationToken>()).Returns(recipe);
    }
}
