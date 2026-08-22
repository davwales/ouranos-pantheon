using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait.Schemas;

public sealed record DeleteTraitResponse(Id<Trait> TraitId);
