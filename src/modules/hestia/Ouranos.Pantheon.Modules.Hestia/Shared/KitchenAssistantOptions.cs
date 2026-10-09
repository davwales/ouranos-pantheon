using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hestia.Shared;

public sealed record KitchenAssistantOptions(
    string ModelName,
    int MaxTokens,
    float Temperature,
    ReasoningEffort ReasoningEffort
)
{
    public KitchenAssistantOptions()
        : this(
            ModelName: ModelDefaults.ModelName,
            MaxTokens: ModelDefaults.MaxTokens,
            Temperature: 0.7f,
            ReasoningEffort: ModelDefaults.ReasoningEffort
        ) { }
}
