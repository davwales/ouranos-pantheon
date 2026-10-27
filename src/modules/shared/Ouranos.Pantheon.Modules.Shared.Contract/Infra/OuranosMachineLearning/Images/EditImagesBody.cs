using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Images;

// No size: OuranosMl then keeps the source image's aspect ratio.
internal sealed record EditImagesBody(
    string Prompt,
    string? Model,
    int N,
    string Image,
    float Strength,
    IReadOnlyList<ImageLoraRequest> Loras,
    bool Stream,
    bool Save,
    bool Previews
);
