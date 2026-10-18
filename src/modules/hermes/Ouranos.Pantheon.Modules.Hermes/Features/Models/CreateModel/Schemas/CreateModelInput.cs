using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel.Schemas;

public sealed record CreateModelInput(
    string Name,
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature = null,
    int? MaxTokens = null,
    Id<ToolSet>? DefaultToolSetId = null,
    int? ContextWindow = null,
    bool IsDefault = false,
    bool IsPublic = true
);
