using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableLoras;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAvailableLoras.Schemas;

public sealed record GetAvailableLorasResponse(
    Id<AvailableLora> Id,
    string Identifier,
    string? Title,
    string? CompatClass,
    float? DefaultWeight,
    string? TriggerPhrase,
    string? UsageHint,
    bool IsCompatible
);
