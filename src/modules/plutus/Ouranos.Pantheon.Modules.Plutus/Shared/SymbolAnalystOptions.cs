using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Plutus.Shared;

public sealed record SymbolAnalystOptions(
    string ModelName,
    int MaxTokens,
    float Temperature,
    ReasoningEffort ReasoningEffort,
    int ChartBuckets,
    int MaxPositions,
    int ContextCacheMinutes
)
{
    public SymbolAnalystOptions()
        : this(
            ModelName: ModelDefaults.ModelName,
            MaxTokens: ModelDefaults.MaxTokens,
            Temperature: 0.3f,
            ReasoningEffort: ModelDefaults.ReasoningEffort,
            ChartBuckets: 24,
            MaxPositions: 20,
            ContextCacheMinutes: 5
        ) { }
}
