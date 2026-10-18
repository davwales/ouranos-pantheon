using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;

public sealed record ResolvedToolSet(
    IReadOnlyList<IAssistantTool<HermesChatContext>> Tools,
    string? Instructions
)
{
    public static ResolvedToolSet Empty { get; } = new([], null);
}
