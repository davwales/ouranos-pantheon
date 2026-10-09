using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Hestia.Shared;

public sealed record RecipeImportOptions(
    string ModelName,
    int MaxTokens,
    float Temperature,
    ReasoningEffort ReasoningEffort
)
{
    public RecipeImportOptions()
        : this(
            ModelName: ModelDefaults.ModelName,
            MaxTokens: ModelDefaults.MaxTokens,
            Temperature: 0f,
            ReasoningEffort: ReasoningEffort.Low
        ) { }
}
