namespace Ouranos.Pantheon.Modules.Hermes.Shared;

/// <summary>
/// The img2img strength (0-1, how far an edit may move away from its source) for each change the
/// model can ask for. Small models pick floats badly, so they choose a named change instead.
/// </summary>
public sealed record HermesImageEditStrengths
{
    public float Subtle { get; init; } = 0.3f;

    public float Moderate { get; init; } = 0.55f;

    public float Strong { get; init; } = 0.75f;
}
