using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora.Schemas;

public sealed record CreateLoraInput(
    string Name,
    string Identifier,
    float Weight = Lora.DefaultWeight,
    bool IsPublic = true,
    string? TriggerPhrase = null
);
