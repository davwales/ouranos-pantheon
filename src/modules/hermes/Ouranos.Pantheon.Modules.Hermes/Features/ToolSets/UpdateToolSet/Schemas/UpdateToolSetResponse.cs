using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet.Schemas;

public sealed record UpdateToolSetResponse(Id<ToolSet> ToolSetId);
