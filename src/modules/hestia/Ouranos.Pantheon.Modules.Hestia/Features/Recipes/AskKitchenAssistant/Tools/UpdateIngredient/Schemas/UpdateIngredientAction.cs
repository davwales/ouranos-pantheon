using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.UpdateIngredient.Schemas;

public sealed record UpdateIngredientAction(
    ProposedIngredient Original,
    ProposedIngredient Updated
);
