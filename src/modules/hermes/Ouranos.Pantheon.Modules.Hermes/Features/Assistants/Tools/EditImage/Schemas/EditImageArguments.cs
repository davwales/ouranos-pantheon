using System.ComponentModel;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.EditImage.Schemas;

public sealed record EditImageArguments(
    [Description(
        "A detailed visual description in English of the whole image you want, not just the change: "
            + "start from the image's prompt and revise it."
    )]
        string Prompt,
    ImageChange Change,
    [Description(
        "The image to change, such as \"image 2\"; omit for the image the user attached, or else the latest image."
    )]
        string? Image
);
