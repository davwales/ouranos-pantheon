namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;

internal sealed record GenerateImagesBody(
    string Prompt,
    string? Model,
    int N,
    string Size,
    bool Stream,
    bool Save,
    bool Previews
);
