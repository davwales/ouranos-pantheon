using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Traits;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits.Schemas;

public sealed record GetAllTraitsResponse(Id<Trait> Id, string Name, string Content, bool IsPublic);
