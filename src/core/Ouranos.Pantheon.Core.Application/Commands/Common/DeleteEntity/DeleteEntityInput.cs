using MediatR;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;

public sealed record DeleteEntityInput<T>(Id<T> EntityId) : IRequest<IdResponse<T>>
    where T : BaseEntity<Id<T>>;