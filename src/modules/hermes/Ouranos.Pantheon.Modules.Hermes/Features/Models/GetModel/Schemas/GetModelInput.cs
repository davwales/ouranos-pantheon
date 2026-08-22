using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Models;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel.Schemas;

public sealed record GetModelInput(Id<ModelConfig> ModelId);
