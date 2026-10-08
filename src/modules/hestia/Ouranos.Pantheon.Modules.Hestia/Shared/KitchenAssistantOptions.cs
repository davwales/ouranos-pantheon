namespace Ouranos.Pantheon.Modules.Hestia.Shared;

public sealed record KitchenAssistantOptions(string ModelName, int MaxTokens, float Temperature)
{
    public KitchenAssistantOptions()
        : this(
            ModelName: "hf.co/nvidia/NVIDIA-Nemotron-3-Nano-4B-GGUF",
            MaxTokens: 2048,
            Temperature: 0.7f
        ) { }
}
