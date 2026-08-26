using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait.Schemas;

public sealed record CreateTraitResponse(Id<Trait> TraitId);
