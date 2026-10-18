namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet.Schemas;

public sealed record CreateToolSetInput(
    string Name,
    string? Description = null,
    string? Instructions = null,
    List<CreateToolSetToolInput>? Tools = null,
    bool IsPublic = true
);
