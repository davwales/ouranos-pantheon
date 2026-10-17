namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;

internal sealed record ImageGenerationCompletedEvent(
    string B64Json,
    long? Seed,
    string Size,
    string OutputFormat
);
