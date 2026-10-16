namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.UpdateIngredient.Schemas;

public sealed record UpdateIngredientArguments(
    int Number,
    decimal? Quantity,
    string Unit,
    string Name,
    bool Optional
);
