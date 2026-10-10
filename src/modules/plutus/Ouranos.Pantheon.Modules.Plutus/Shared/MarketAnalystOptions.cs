using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Plutus.Shared;

// The inference model's context window is small, so the output budget and reasoning effort are
// reduced to leave room for the prompt and the conversation history.
public sealed record MarketAnalystOptions(
    string ModelName,
    int MaxTokens,
    float Temperature,
    ReasoningEffort ReasoningEffort,
    int MaxViewRows,
    int TrendPoints,
    int ContextCacheMinutes
)
{
    public MarketAnalystOptions()
        : this(
            ModelName: ModelDefaults.ModelName,
            MaxTokens: 1536,
            Temperature: 0.3f,
            ReasoningEffort: ModelDefaults.ReasoningEffort,
            MaxViewRows: 12,
            TrendPoints: 6,
            ContextCacheMinutes: 5
        ) { }
}
