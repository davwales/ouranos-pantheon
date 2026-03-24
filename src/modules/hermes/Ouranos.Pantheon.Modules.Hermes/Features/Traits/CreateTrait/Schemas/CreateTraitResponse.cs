using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait.Schemas;

public sealed record CreateTraitResponse(
    Id<Trait> TraitId
);
