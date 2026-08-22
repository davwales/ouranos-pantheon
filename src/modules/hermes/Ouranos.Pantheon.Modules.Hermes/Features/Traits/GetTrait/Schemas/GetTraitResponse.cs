using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait.Schemas;

public sealed record GetTraitResponse(Id<Trait> Id, string Name, string Content, bool IsPublic);
