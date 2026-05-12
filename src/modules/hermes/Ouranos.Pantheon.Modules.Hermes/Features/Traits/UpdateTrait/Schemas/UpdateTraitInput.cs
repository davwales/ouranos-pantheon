using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;

public sealed record UpdateTraitInput(
    Id<Trait> TraitId,
    string Name,
    string Content,
    bool IsPublic = true
);
