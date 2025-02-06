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
        var mockMediatorRegistrationConfigurator = new Mock<IMediatorRegistrationConfigurator>();

        // Act
        mockMediatorRegistrationConfigurator.Object.AddStandardConsumersForEntity<TestEntity>();

        // Assert
        mockMediatorRegistrationConfigurator.Verify(
            m => m.AddConsumer<GetAllEntitiesHandler<TestEntity>>(null),
            Times.Once
        );

        mockMediatorRegistrationConfigurator.Verify(
            m => m.AddConsumer<GetEntityHandler<TestEntity>>(null),
            Times.Once
        );

        mockMediatorRegistrationConfigurator.Verify(
            m => m.AddConsumer<DeleteEntityHandler<TestEntity>>(null),
            Times.Once
        );
    }
}