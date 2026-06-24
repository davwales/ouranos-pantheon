using Marten;
using Marten.Schema;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.ValueTypes;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Database;

public sealed class HestiaRecipeSeedData : IInitialData
{
    public static readonly RecipeDocument CinnamonSugar = new()
    {
        Id = Guid.Parse("68cd9db4-17de-4209-bb87-decb97e8d68b"),
        Title = "Cinnamon Sugar",
        Instructions =
            "In a small bowl, mix both ingredients together. Store in a dry location indefinitely.",
        Ingredients =
        [
            new Ingredient(4m, "tablespoons", "granulated sugar"),
            new Ingredient(1m, "tablespoon", "ground cinnamon"),
        ],
        Notes = "Great on toast.",
        CreatedAt = DateTimeOffset.UtcNow,
    };

    public async Task Populate(IDocumentStore store, CancellationToken cancellation)
    {
        using var session = store.LightweightSession();
        var hasAny = await session.Query<RecipeDocument>().AnyAsync(cancellation);
        if (hasAny)
        {
            return;
        }

        session.Store(CinnamonSugar);
        await session.SaveChangesAsync(cancellation);
    }
}
