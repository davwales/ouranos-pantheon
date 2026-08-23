using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;

public sealed record ImportRecipeRequested(
    Id<Recipe> RecipeId,
    string Url,
    DateTimeOffset RequestedAt
)
{
    public const string Exchange = "hestia.recipe";
    public const string Queue = "hestia.recipe.import";
    public const string DeadLetterQueue = "hestia.recipe.import.dlq";
}
