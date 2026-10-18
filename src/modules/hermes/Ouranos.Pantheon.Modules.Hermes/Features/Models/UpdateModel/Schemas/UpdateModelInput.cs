using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel.Schemas;

public sealed record UpdateModelInput(
    Id<ModelConfig> ModelId,
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
