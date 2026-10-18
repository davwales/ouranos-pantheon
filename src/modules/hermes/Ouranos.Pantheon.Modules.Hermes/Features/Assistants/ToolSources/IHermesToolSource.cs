using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;

public interface IHermesToolSource
{
    ToolSource Source { get; }

    IReadOnlyList<AvailableTool> ListTools();

    IReadOnlyList<IAssistantTool<HermesChatContext>> Resolve(IEnumerable<ToolSetTool> entries);
}
