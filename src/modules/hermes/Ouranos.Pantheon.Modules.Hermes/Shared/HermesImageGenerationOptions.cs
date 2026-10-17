namespace Ouranos.Pantheon.Modules.Hermes.Shared;

public sealed record HermesImageGenerationOptions
{
    public const int DefaultMaxPromptLength = 1000;

    public bool IsEnabled { get; init; } = true;

    /// <summary>
    /// The OuranosMl image model; null uses OuranosMl's default.
    /// </summary>
    public string? Model { get; init; }

    /// <summary>
    /// How long an image no saved conversation references is kept, which bounds how long an
    /// unsaved chat's images outlive it.
    /// </summary>
    public TimeSpan UnsavedRetention { get; init; } = TimeSpan.FromHours(24);

    public int MaxPromptLength { get; init; } = DefaultMaxPromptLength;

    // The defaults are multiples of 8 near one megapixel, which SDXL- and Flux-class models are
    // trained around; a model trained at another resolution needs its own sizes.
    public HermesImageSize SquareSize { get; init; } = new(1024, 1024);

    public HermesImageSize PortraitSize { get; init; } = new(832, 1216);

    public HermesImageSize LandscapeSize { get; init; } = new(1216, 832);
}
