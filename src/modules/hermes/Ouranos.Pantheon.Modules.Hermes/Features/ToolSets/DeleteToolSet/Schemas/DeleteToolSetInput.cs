using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet.Schemas;

public sealed record DeleteToolSetInput(Id<ToolSet> ToolSetId);
