using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits.Schemas;

public sealed record GetAllTraitsResponse(
    Id<Trait> Id,
    string Name,
    string Content,
    bool IsPublic
);
