using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;

public sealed class GeneratedImage : BaseEntity<Id<GeneratedImage>>
{
    private GeneratedImage(Id<GeneratedImage> id)
        : base(id)
    {
        Content = [];
        MediaType = string.Empty;
        Prompt = string.Empty;
        Model = string.Empty;
    }

    public byte[] Content { get; private set; }

    public string MediaType { get; private set; }

    public int Width { get; private set; }

    public int Height { get; private set; }

    public string Prompt { get; private set; }

    public string Model { get; private set; }

    public long? Seed { get; private set; }

    public static GeneratedImage Create(
        Id<GeneratedImage> id,
        byte[] content,
        string mediaType,
        int width,
        int height,
        string prompt,
        string model,
        long? seed = null
    )
    {
        Guard.Against.NullOrEmpty(content);
        Guard.Against.NullOrWhiteSpace(mediaType);
        Guard.Against.NegativeOrZero(width);
        Guard.Against.NegativeOrZero(height);
        Guard.Against.NullOrWhiteSpace(prompt);
        Guard.Against.Null(model);

        return new GeneratedImage(id)
        {
            Content = content,
            MediaType = mediaType,
            Width = width,
            Height = height,
            Prompt = prompt,
            Model = model,
            Seed = seed,
        };
    }
}
