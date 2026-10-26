using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;

/// <summary>
/// The images a chat turn can refer to, numbered from 1 in the order they first appear. The
/// numbers are worked out from the history on every turn and never stored, so they stay right
/// when messages are edited or deleted.
/// </summary>
public sealed class ConversationImages
{
    public static readonly ConversationImages Empty = new([], null, null);

    private readonly Dictionary<Id<GeneratedImage>, ConversationImage> _byId;

    public ConversationImages(
        IReadOnlyList<ConversationImage> images,
        ConversationImage? attached,
        ConversationImage? latest
    )
    {
        Images = images;
        Attached = attached;
        Latest = latest;
        _byId = images.ToDictionary(i => i.Id);
    }

    public IReadOnlyList<ConversationImage> Images { get; }

    /// <summary>
    /// The image the user attached to the message this turn answers, if any.
    /// </summary>
    public ConversationImage? Attached { get; }

    /// <summary>
    /// The image shown or attached last.
    /// </summary>
    public ConversationImage? Latest { get; }

    public ConversationImage? Find(Id<GeneratedImage> id)
    {
        return _byId.GetValueOrDefault(id);
    }

    public ConversationImage? FindByRef(int imageRef)
    {
        return imageRef >= 1 && imageRef <= Images.Count ? Images[imageRef - 1] : null;
    }
}
