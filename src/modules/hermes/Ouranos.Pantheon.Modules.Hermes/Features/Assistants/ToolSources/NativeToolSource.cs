using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;

public sealed class NativeToolSource : IHermesToolSource
{
    private readonly IReadOnlyList<IAssistantTool<HermesChatContext>> _tools;

    public NativeToolSource(IEnumerable<IAssistantTool<HermesChatContext>> tools)
    {
        Guard.Against.Null(tools);

        _tools = [.. tools];
    }

    public ToolSource Source => ToolSource.Native;

    public IReadOnlyList<AvailableTool> ListTools()
    {
        return [.. _tools.Select(t => new AvailableTool(Source, t.Name, t.Description))];
    }

    public IReadOnlyList<IAssistantTool<HermesChatContext>> Resolve(
        IEnumerable<ToolSetTool> entries
    )
    {
        Guard.Against.Null(entries);

        List<IAssistantTool<HermesChatContext>> resolved = [];
        foreach (var entry in entries.Where(e => e.Source == Source))
        {
            var tool = _tools.FirstOrDefault(t => t.Name == entry.Name);
            if (tool is not null)
            {
                resolved.Add(new ToolSetAssistantTool(tool, entry.AdditionalDescription));
            }
        }

        return resolved;
    }
}
