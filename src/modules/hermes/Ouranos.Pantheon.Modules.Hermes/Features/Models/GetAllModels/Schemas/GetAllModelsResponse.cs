using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels.Schemas;

public sealed record GetAllModelsResponse(
    Id<ModelConfig> Id,
    string Name,
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature,
    int? MaxTokens,
    float? RepeatPenalty,
    int? ContextWindow,
    bool IsDefault,
    bool IsPublic
);
