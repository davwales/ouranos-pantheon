namespace Ouranos.Pantheon.Modules.Hestia.Shared;

public sealed record RecipeImportOptions(string ModelName, int MaxTokens, float Temperature)
{
    public RecipeImportOptions()
        : this(
            ModelName: "hf.co/nvidia/NVIDIA-Nemotron-3-Nano-4B-GGUF",
            MaxTokens: 4096,
            Temperature: 0f
        ) { }
}
