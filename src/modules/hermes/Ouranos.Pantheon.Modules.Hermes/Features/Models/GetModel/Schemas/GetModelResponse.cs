using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel.Schemas;

public sealed record GetModelResponse(
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
