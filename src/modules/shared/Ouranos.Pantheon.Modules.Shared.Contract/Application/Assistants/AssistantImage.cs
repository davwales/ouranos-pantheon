namespace Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;

/// <summary>
/// An image a tool made for the user. <see cref="Url"/> is relative to the gateway; the model
/// never sees the image, only the tool's output text.
/// </summary>
public sealed record AssistantImage(string Id, string Url, int Width, int Height, string AltText);
