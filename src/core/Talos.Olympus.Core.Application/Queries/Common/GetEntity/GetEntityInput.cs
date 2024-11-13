using MediatR;
using Talos.Olympus.Core.Domain.Common;

namespace Talos.Olympus.Core.Application.Queries.Common.GetEntity;

public sealed record GetEntityInput<T>(Id<T> EntityId) : IRequest<T> where T : BaseEntity<Id<T>>;