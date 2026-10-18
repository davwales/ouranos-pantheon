using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools.Schemas;

public sealed record GetAvailableToolsResponse(ToolSource Source, string Name, string Description);
