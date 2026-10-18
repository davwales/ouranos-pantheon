using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet.Schemas;

public sealed record UpdateToolSetToolInput(
    ToolSource Source,
    string Name,
    string? AdditionalDescription = null
);
