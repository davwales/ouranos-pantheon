using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

public sealed class ToolSetTool : BaseEntity<Id<ToolSetTool>>
{
    public const int MaxAdditionalDescriptionLength = 500;

    private ToolSetTool(Id<ToolSetTool> id)
        : base(id)
    {
        Name = string.Empty;
    }

    public Id<ToolSet> ToolSetId { get; private set; }

    public ToolSource Source { get; private set; }

    public string Name { get; private set; }

    public string? AdditionalDescription { get; private set; }

    public static ToolSetTool Create(
        Id<ToolSetTool> id,
        Id<ToolSet> toolSetId,
        ToolSource source,
        string name,
        string? additionalDescription = null
    )
    {
        Guard.Against.Null(toolSetId);
        Guard.Against.EnumOutOfRange(source);
        Guard.Against.NullOrWhiteSpace(name);

        return new ToolSetTool(id)
        {
            ToolSetId = toolSetId,
            Source = source,
            Name = name.Trim(),
            AdditionalDescription = NormalizeAdditionalDescription(additionalDescription),
        };
    }

    public bool IsSameTool(ToolSetTool other)
    {
        Guard.Against.Null(other);

        return Source == other.Source && string.Equals(Name, other.Name, StringComparison.Ordinal);
    }

    public void Update(string? additionalDescription)
    {
        AdditionalDescription = NormalizeAdditionalDescription(additionalDescription);
        Update();
    }

    private static string? NormalizeAdditionalDescription(string? additionalDescription)
    {
        if (string.IsNullOrWhiteSpace(additionalDescription))
        {
            return null;
        }

        var trimmed = additionalDescription.Trim();
        Guard.Against.StringTooLong(trimmed, MaxAdditionalDescriptionLength);
        return trimmed;
    }
}
