using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora.Schemas;

public sealed record UpdateLoraInput(
    Id<Lora> LoraId,
    string Name,
    string Identifier,
    float Weight = Lora.DefaultWeight,
    bool IsPublic = true,
    string? TriggerPhrase = null
);
