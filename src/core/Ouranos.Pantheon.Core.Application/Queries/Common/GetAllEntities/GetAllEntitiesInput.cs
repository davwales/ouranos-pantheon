using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;

public sealed record GetAllEntitiesInput<T> : IQuery<WrapperResponse<IQueryable<T>>> where T : BaseEntity<Id<T>>;