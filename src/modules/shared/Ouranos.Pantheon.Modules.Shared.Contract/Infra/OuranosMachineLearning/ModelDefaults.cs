using Effort = Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos.ReasoningEffort;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

public static class ModelDefaults
{
    public const string ModelName = "hf.co/nvidia/NVIDIA-Nemotron-3-Nano-4B-GGUF";

    public const Effort ReasoningEffort = Effort.Medium;

    // The Responses API counts reasoning tokens against max_output_tokens, so the budget must
    // leave room for the model to think before it answers.
    public const int MaxTokens = 8192;
}
