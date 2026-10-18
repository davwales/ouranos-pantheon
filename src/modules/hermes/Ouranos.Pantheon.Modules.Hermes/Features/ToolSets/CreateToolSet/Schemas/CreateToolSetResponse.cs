using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet.Schemas;

public sealed record CreateToolSetResponse(Id<ToolSet> ToolSetId);
