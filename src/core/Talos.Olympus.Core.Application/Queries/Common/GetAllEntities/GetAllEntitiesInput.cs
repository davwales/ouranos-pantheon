using MediatR;
using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Application.Queries.Common.GetAllEntities;

public sealed record GetAllEntitiesInput<T> : IRequest<IQueryable<T>> where T : BaseEntity<Id<T>>;