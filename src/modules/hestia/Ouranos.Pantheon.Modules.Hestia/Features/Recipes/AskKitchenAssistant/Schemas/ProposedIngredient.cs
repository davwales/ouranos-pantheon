namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Schemas;

public sealed record ProposedIngredient(
    decimal Quantity,
    string Unit,
    string Name,
    bool IsOptional
);
