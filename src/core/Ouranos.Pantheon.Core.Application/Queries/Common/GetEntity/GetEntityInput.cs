using MediatR;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;

public sealed record GetEntityInput<T>(Id<T> EntityId) : IRequest<T> where T : BaseEntity<Id<T>>;