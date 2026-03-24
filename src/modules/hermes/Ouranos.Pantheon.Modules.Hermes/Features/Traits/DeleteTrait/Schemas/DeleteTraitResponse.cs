using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait.Schemas;

public sealed record DeleteTraitResponse(
    Id<Trait> TraitId
);
