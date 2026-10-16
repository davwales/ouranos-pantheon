using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Plutus.Shared;

public sealed record SymbolAnalystOptions(
    string ModelName,
    int MaxTokens,
    float Temperature,
    ReasoningEffort ReasoningEffort,
    int ChartPoints,
    int MaxPositions,
    int ContextCacheMinutes
)
{
    public SymbolAnalystOptions()
        : this(
            ModelName: ModelDefaults.ModelName,
            MaxTokens: 4096,
            Temperature: 0.3f,
            ReasoningEffort: ModelDefaults.ReasoningEffort,
            ChartPoints: 8,
            MaxPositions: 10,
            ContextCacheMinutes: 5
        ) { }
}
