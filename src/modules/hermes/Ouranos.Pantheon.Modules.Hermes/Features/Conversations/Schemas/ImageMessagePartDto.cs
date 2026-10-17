using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Conversations.Schemas;

public sealed record ImageMessagePartDto(
    Id<GeneratedImage> ImageId,
    int Width,
    int Height,
    string AltText
) : MessagePartDto;
