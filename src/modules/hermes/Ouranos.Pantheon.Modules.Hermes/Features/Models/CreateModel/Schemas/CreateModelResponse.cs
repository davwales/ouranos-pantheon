using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel.Schemas;

public sealed record CreateModelResponse(Id<ModelConfig> ModelId);
