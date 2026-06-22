using Marten.Events.Aggregation;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;

namespace Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Projections;

public partial class RecipeProjection : SingleStreamProjection<RecipeDocument, Guid>
{
    // Initial projection - only handles stream creation. Subsequent Apply methods
    // (RecipeIngredientAdded, RecipeInstructionsUpdated, etc.) will be added in
    // follow-up tickets as additional event types are introduced.

    public static RecipeDocument Create(RecipeCreated created)
    {
        return new() { Id = created.Id, Title = created.Title };
    }
}
