using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources.Schemas;

public sealed record AvailableTool(ToolSource Source, string Name, string Description);
