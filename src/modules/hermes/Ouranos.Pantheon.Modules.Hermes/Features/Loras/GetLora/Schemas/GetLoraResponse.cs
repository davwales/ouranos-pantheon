using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetLora.Schemas;

public sealed record GetLoraResponse(
    Id<Lora> Id,
    string Name,
    string Identifier,
    float Weight,
    bool IsPublic,
    string? TriggerPhrase,
    bool IsUnavailable,
    bool IsIncompatible
);
