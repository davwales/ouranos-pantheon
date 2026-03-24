using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;

public sealed record UpdateTraitInput(
    Id<Trait> TraitId,
    string Name,
    string Content
);
