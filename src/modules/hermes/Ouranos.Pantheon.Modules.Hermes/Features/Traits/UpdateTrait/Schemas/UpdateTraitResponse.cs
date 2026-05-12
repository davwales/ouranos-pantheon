using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait.Schemas;

public sealed record UpdateTraitResponse(Id<Trait> TraitId);
