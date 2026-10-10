using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

public sealed record AssistantPrompt(
    string ModelIdentifier,
    string SystemPrompt,
    float? Temperature = null,
    int? MaxTokens = null,
    ReasoningEffort? ReasoningEffort = null,
    int HistoryCharacterBudget = ModelDefaults.HistoryCharacterBudget
);
