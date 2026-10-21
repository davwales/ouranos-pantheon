using System.ComponentModel;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage.Schemas;

public sealed record GenerateImageArguments(
    [Description(
        "A detailed visual description in English: subject, style, composition and lighting. "
            + "Every call draws from scratch, so when revising an earlier image write the full revised description."
    )]
        string Prompt,
    [Description("The image shape; omit for a square image.")] ImageOrientation? Orientation
);
