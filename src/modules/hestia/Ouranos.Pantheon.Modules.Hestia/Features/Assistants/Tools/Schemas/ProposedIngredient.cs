namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.Schemas;

public sealed record ProposedIngredient(
    decimal Quantity,
    string Unit,
    string Name,
    bool IsOptional
);
