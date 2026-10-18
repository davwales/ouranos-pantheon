using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets.Schemas;

public sealed record GetAllToolSetsToolResponse(
    ToolSource Source,
    string Name,
    string? AdditionalDescription,
    bool IsAvailable
);
