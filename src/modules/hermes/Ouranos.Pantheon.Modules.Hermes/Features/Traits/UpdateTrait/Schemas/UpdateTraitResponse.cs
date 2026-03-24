using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;

public sealed record UpdateTraitResponse(
    Id<Trait> TraitId
);
