using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;

public sealed record GetEntityInput<T>(Id<T> EntityId) : IQuery<T> where T : BaseEntity<Id<T>>;