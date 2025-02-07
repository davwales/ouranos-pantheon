using MassTransit;
using Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Application.Tests.Mediator;

public sealed class MediatorExtensionsTests
{
    [Fact]
    public void AddStandardConsumersForEntity_ShouldRegisterExpectedConsumers()
    {
        // Arrange
        var mediatorRegistrationConfigurator = Substitute.For<IMediatorRegistrationConfigurator>();

        // Act
        mediatorRegistrationConfigurator.AddStandardConsumersForEntity<TestEntity>();

        // Assert
        mediatorRegistrationConfigurator.Received(1).AddConsumer<GetAllEntitiesHandler<TestEntity>>();
        mediatorRegistrationConfigurator.Received(1).AddConsumer<GetEntityHandler<TestEntity>>();
        mediatorRegistrationConfigurator.Received(1).AddConsumer<DeleteEntityHandler<TestEntity>>();
    }
}