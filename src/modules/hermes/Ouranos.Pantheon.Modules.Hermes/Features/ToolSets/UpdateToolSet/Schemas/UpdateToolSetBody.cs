namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet.Schemas;

public sealed record UpdateToolSetBody(
    string Name,
    string? Description = null,
    string? Instructions = null,
    List<UpdateToolSetToolInput>? Tools = null,
    bool IsPublic = true
);
