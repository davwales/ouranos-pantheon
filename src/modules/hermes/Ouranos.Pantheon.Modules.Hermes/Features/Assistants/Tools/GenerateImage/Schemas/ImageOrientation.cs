using System.ComponentModel;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage.Schemas;

[Description("Portrait is taller than wide, landscape wider than tall.")]
public enum ImageOrientation
{
    Square,
    Portrait,
    Landscape,
}
