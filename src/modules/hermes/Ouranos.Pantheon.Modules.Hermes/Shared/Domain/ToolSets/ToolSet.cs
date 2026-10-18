using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

public class ToolSet : BaseEntity<Id<ToolSet>>
{
    public const int MaxInstructionsLength = 1000;

    protected ToolSet(Id<ToolSet> id)
        : base(id)
    {
        Name = string.Empty;
    }

    public string Name { get; private set; }

    public string? Description { get; private set; }

    public string? Instructions { get; private set; }

    public bool IsPublic { get; private set; }

    public List<ToolSetTool> Tools { get; private set; } = [];

    public static ToolSet Create(
        Id<ToolSet> id,
        string name,
        string? description,
        string? instructions,
        IEnumerable<ToolSetTool> tools,
        bool isPublic = true
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(tools);

        List<ToolSetTool> toolList = [.. tools];
        GuardTools(id, toolList);

        return new ToolSet(id)
        {
            Name = name.Trim(),
            Description = NormalizeOptional(description),
            Instructions = NormalizeInstructions(instructions),
            IsPublic = isPublic,
            Tools = toolList,
        };
    }

    public void Update(
        string name,
        string? description,
        string? instructions,
        IEnumerable<ToolSetTool> tools,
        bool isPublic = true
    )
    {
        Guard.Against.NullOrWhiteSpace(name);
        Guard.Against.Null(tools);

        List<ToolSetTool> toolList = [.. tools];
        GuardTools(Id, toolList);

        Name = name.Trim();
        Description = NormalizeOptional(description);
        Instructions = NormalizeInstructions(instructions);
        IsPublic = isPublic;

        MergeTools(toolList);
        Update();
    }

    private void MergeTools(List<ToolSetTool> desired)
    {
        Tools.RemoveAll(existing => !desired.Exists(existing.IsSameTool));

        foreach (var tool in desired)
        {
            var existing = Tools.Find(t => t.IsSameTool(tool));
            if (existing is null)
            {
                Tools.Add(tool);
            }
            else
            {
                existing.Update(tool.AdditionalDescription);
            }
        }
    }

    private static void GuardTools(Id<ToolSet> id, List<ToolSetTool> tools)
    {
        foreach (var tool in tools)
        {
            Guard.Against.InvalidInput(tool, nameof(tools), t => t.ToolSetId == id);
        }

        Guard.Against.InvalidInput(
            tools,
            nameof(tools),
            t => t.GroupBy(tool => (tool.Source, tool.Name)).All(g => g.Count() == 1),
            "A tool set cannot contain the same tool more than once."
        );
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value) ? null : value.Trim();
    }

    private static string? NormalizeInstructions(string? instructions)
    {
        var normalized = NormalizeOptional(instructions);
        if (normalized is not null)
        {
            Guard.Against.StringTooLong(normalized, MaxInstructionsLength);
        }

        return normalized;
    }
}
