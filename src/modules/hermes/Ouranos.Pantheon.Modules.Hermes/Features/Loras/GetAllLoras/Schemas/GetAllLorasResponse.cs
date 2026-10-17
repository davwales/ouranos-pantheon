using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras.Schemas;

public sealed record GetAllLorasResponse(
    Id<Lora> Id,
    string Name,
    string Identifier,
    float Weight,
    bool IsPublic
);
