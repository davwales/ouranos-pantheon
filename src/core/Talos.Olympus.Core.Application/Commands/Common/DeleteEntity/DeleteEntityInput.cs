using MediatR;
using Talos.Olympus.Core.Application.Common;
using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Application.Commands.Common.DeleteEntity;

public sealed record DeleteEntityInput<T>(Id<T> EntityId) : IRequest<IdResponse<T>>
    where T : BaseEntity<Id<T>>;