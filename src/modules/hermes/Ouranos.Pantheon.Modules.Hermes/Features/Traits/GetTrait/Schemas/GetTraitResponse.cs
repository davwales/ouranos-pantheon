using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait.Schemas;

public sealed record GetTraitResponse(
    Id<Trait> Id,
    string Name,
    string Content
);
