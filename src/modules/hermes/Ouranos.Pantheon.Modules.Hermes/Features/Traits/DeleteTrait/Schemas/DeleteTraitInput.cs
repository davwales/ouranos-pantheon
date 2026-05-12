using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait.Schemas;

public sealed record DeleteTraitInput(Id<Trait> TraitId);
