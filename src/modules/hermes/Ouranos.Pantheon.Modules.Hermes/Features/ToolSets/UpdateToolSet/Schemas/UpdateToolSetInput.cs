using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet.Schemas;

public sealed record UpdateToolSetInput(
    Id<ToolSet> ToolSetId,
    string Name,
    string? Description = null,
    string? Instructions = null,
    List<UpdateToolSetToolInput>? Tools = null,
    bool IsPublic = true
);
