namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.AddIngredient.Schemas;

public sealed record AddIngredientArguments(
    decimal? Quantity,
    string Unit,
    string Name,
    bool Optional
);
