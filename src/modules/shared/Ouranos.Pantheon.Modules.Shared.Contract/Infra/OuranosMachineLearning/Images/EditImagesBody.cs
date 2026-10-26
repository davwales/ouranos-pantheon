namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;

// No size: OuranosMl then keeps the source image's aspect ratio.
internal sealed record EditImagesBody(
    string Prompt,
    string? Model,
    int N,
    string Image,
    float Strength,
    bool Stream,
    bool Save,
    bool Previews
);
