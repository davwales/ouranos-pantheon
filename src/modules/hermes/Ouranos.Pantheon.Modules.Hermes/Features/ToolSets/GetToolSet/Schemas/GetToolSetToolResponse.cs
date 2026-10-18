using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet.Schemas;

public sealed record GetToolSetToolResponse(
    ToolSource Source,
    string Name,
    string? AdditionalDescription,
    bool IsAvailable
);
