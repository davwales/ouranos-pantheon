using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel.Schemas;

public sealed record DeleteModelResponse(
    Id<ModelConfig> ModelId
);
