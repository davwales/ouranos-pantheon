using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets.Schemas;

public sealed record GetAllToolSetsResponse(
    Id<ToolSet> Id,
    string Name,
    string? Description,
    List<GetAllToolSetsToolResponse> Tools,
    bool IsPublic
);
