namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;

/// <summary>
/// The SSE event names OuranosMl's streamed <c>POST /images/generations</c> sends.
/// </summary>
internal static class ImageGenerationEventTypes
{
    public const string Completed = "image_generation.completed";
    public const string Done = "image_generation.done";
    public const string Error = "error";
}
