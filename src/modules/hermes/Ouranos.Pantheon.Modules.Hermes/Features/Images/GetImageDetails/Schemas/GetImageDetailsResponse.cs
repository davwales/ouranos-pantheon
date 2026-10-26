using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails.Schemas;

public sealed record GetImageDetailsResponse(
    Id<GeneratedImage> Id,
    int Width,
    int Height,
    string Prompt,
    string Model,
    long? Seed,
    Id<GeneratedImage>? SourceImageId,
    DateTimeOffset CreatedAt
);
