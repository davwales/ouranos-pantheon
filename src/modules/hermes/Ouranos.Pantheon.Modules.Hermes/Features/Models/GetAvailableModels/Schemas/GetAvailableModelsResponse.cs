using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.AvailableModels;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels.Schemas;

public sealed record GetAvailableModelsResponse(
    Id<AvailableModel> Id,
    string ModelIdentifier,
    string OwnedBy
);
