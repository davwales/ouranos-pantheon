using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel.Schemas;

public sealed record DeleteModelInput(Id<ModelConfig> ModelId);
