using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Images;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImageDetails.Schemas;

public sealed record GetImageDetailsInput(Id<GeneratedImage> ImageId);
