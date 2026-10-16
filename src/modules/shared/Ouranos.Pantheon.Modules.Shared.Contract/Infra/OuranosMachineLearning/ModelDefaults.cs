using Effort = Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos.ReasoningEffort;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

public static class ModelDefaults
{
    public const string ModelName = "hf.co/nvidia/NVIDIA-Nemotron-3-Nano-4B-GGUF";

    public const Effort ReasoningEffort = Effort.Medium;

    // The Responses API counts reasoning tokens against max_output_tokens, so the budget must
    // leave room for the model to think before it answers.
    public const int MaxTokens = 8192;

    // The model's context window is small and shared by the system prompt, the conversation, the
    // reasoning and the reply, so assistants send only the most recent turns that fit this budget.
    public const int HistoryCharacterBudget = 4000;

    // Each tool round resends the whole conversation, so rounds are capped; the last round offers
    // no tools, which forces the model to answer with what it has.
    public const int MaxToolRounds = 3;

    // Small models sometimes repeat a call many times in one round; each call costs a query and
    // can produce an action, so only the first few calls in a round run.
    public const int MaxToolCallsPerRound = 4;

    public const int MaxToolOutputCharacters = 1500;
}
