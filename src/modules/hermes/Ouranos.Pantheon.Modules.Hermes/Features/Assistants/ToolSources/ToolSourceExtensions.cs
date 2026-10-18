using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;

public static class ToolSourceExtensions
{
    public static IReadOnlyList<AvailableTool> ListAllTools(
        this IEnumerable<IHermesToolSource> sources
    )
    {
        return [.. sources.SelectMany(s => s.ListTools())];
    }

    public static bool Includes(
        this IReadOnlyList<AvailableTool> tools,
        ToolSource source,
        string name
    )
    {
        return tools.Any(t => t.Source == source && t.Name == name?.Trim());
    }
}
