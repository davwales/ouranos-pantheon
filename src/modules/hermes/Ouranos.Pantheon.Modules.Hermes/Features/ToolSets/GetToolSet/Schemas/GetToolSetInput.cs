using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet.Schemas;

public sealed record GetToolSetInput(Id<ToolSet> ToolSetId);
