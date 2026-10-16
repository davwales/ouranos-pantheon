using Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.Schemas;

namespace Ouranos.Pantheon.Modules.Hestia.Features.Assistants.Tools.UpdateIngredient.Schemas;

public sealed record UpdateIngredientAction(
    ProposedIngredient Original,
    ProposedIngredient Updated
);
