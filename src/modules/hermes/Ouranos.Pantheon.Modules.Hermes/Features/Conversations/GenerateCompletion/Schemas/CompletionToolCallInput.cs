using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;

public sealed record CompletionToolCallInput(
    string CallId,
    string Name,
    string Arguments,
    string Output,
    // The image the call showed, if any.
    Id<GeneratedImage>? ImageId = null
);
