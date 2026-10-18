using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet.Schemas;

public sealed record CreateToolSetToolInput(
    ToolSource Source,
    string Name,
    string? AdditionalDescription = null
);
