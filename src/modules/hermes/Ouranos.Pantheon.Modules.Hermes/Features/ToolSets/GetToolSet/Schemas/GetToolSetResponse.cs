using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet.Schemas;

public sealed record GetToolSetResponse(
    Id<ToolSet> Id,
    string Name,
    string? Description,
    string? Instructions,
    List<GetToolSetToolResponse> Tools,
    bool IsPublic
);
