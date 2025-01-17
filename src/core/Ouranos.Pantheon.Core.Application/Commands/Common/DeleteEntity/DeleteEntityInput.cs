using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;

public sealed record DeleteEntityInput<T>(Id<T> EntityId) : ICommand<IdResponse<T>> where T : BaseEntity<Id<T>>;