using MediatR;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;

public sealed record GetAllEntitiesInput<T> : IRequest<IQueryable<T>> where T : BaseEntity<Id<T>>;