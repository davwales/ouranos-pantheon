using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Conversations;

/// <summary>
/// An image shown in a reply. It references the image without a foreign key: the image may have
/// expired before an unsaved chat is saved, and the part is still kept so the client can say so.
/// </summary>
public sealed class ImageMessagePart : MessagePart
{
    private ImageMessagePart(Id<MessagePart> id)
        : base(id)
    {
        AltText = string.Empty;
    }

    public Id<GeneratedImage> ImageId { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public string AltText { get; private set; }

    public static ImageMessagePart Create(
        Id<MessagePart> id,
        Id<GeneratedImage> imageId,
        int width,
        int height,
        string altText
    )
    {
        Guard.Against.Default(imageId);
        Guard.Against.NegativeOrZero(width);
        Guard.Against.NegativeOrZero(height);
        Guard.Against.Null(altText);

        return new ImageMessagePart(id)
        {
            ImageId = imageId,
            Width = width,
            Height = height,
            AltText = altText,
        };
    }
}
