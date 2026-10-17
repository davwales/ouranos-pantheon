using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Loras;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora.Schemas;

public sealed record UpdateLoraBody(
    string Name,
    string Identifier,
    float Weight = Lora.DefaultWeight,
    bool IsPublic = true
);
