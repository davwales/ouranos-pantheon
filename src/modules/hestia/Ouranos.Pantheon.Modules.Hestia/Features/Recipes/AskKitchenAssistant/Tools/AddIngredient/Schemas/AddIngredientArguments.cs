namespace Ouranos.Pantheon.Modules.Hestia.Features.Recipes.AskKitchenAssistant.Tools.AddIngredient.Schemas;

public sealed record AddIngredientArguments(
    decimal? Quantity,
    string Unit,
    string Name,
    bool Optional
);
