using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.ToolSets;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels.Schemas;

public sealed record GetAllModelsResponse(
    Id<ModelConfig> Id,
    string Name,
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature,
    int? MaxTokens,
    Id<ToolSet>? DefaultToolSetId,
    int? ContextWindow,
    bool IsDefault,
    bool IsPublic,
    bool IsUnavailable
);
