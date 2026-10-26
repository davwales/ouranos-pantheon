using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Scopes;

/// <summary>
/// An image in the chat, which the model refers to as <c>image {Ref}</c>.
/// </summary>
public sealed record ConversationImage(int Ref, Id<GeneratedImage> Id, string Prompt);
