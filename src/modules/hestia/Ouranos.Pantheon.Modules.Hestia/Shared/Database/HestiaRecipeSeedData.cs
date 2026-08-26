using Marten;
using Marten.Schema;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Database;

public sealed class HestiaRecipeSeedData : IInitialData
{
    public static readonly RecipeCreated CinnamonSugarEvent = new(
        Guid.Parse("68cd9db4-17de-4209-bb87-decb97e8d68b"),
        "Cinnamon Sugar",
        null,
        [
            new Step(
                "In a small bowl, mix both ingredients together. Store in a dry location indefinitely."
            ),
        ],
        [
            new Ingredient(4m, "tablespoons", "granulated sugar"),
            new Ingredient(1m, "tablespoon", "ground cinnamon"),
        ],
        "Great on toast.",
        DateTimeOffset.UtcNow
    );

    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();
        var hasAny = await session.Query<Recipe>().AnyAsync(cancellation);
        if (hasAny)
        {
            return;
        }

        session.Events.StartStream(CinnamonSugarEvent.Id, CinnamonSugarEvent);
        await session.SaveChangesAsync(cancellation);
    }
}
