using MassTransit;
using Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Core.Domain.Common;

namespace Ouranos.Pantheon.Core.Application.Mediator;

public static class MediatorExtensions
{
    public static IMediatorRegistrationConfigurator AddStandardConsumersForEntity<T>(
        this IMediatorRegistrationConfigurator mediator
    ) where T : BaseEntity<Id<T>>
    {
        mediator.AddConsumer<GetAllEntitiesHandler<T>>();
        mediator.AddConsumer<GetEntityHandler<T>>();
        mediator.AddConsumer<DeleteEntityHandler<T>>();

        return mediator;
    }
}