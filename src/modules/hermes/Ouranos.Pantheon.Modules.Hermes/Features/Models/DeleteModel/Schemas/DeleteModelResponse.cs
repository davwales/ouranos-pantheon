using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel.Schemas;

public sealed record DeleteModelResponse(Id<ModelConfig> ModelId);
