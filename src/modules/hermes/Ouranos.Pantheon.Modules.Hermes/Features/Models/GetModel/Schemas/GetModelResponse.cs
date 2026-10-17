using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel.Schemas;

public sealed record GetModelResponse(
    Id<ModelConfig> Id,
    string Name,
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature,
    int? MaxTokens,
    bool IsToolCallingEnabled,
    int? ContextWindow,
    bool IsDefault,
    bool IsPublic,
    bool IsUnavailable
);
