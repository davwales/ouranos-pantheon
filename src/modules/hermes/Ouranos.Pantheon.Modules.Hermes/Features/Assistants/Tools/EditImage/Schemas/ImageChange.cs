using System.ComponentModel;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.EditImage.Schemas;

[Description(
    "How far to move away from the image: subtle for a close variation or small fix, moderate for "
        + "a clear change such as a new time of day or outfit, strong to rework most of it."
)]
public enum ImageChange
{
    Subtle,
    Moderate,
    Strong,
}
