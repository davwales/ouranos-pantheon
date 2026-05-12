using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel.Schemas;

public sealed record UpdateModelResponse(Id<ModelConfig> ModelId);
