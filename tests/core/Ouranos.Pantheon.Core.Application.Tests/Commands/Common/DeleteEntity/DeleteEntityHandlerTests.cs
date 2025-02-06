using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Commands.Common.DeleteEntity;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Application.Tests.Commands.Common.DeleteEntity;

public sealed class DeleteEntityHandlerTests
{
    private readonly DeleteEntityHandler<TestEntity> _handler;
    private readonly Mock<IRepository<TestEntity>> _mockCrudRepository = new();
    private readonly Mock<ILogger<DeleteEntityHandler<TestEntity>>> _mockLogger = new();

    public DeleteEntityHandlerTests()
    {
        _handler = new DeleteEntityHandler<TestEntity>(
            _mockLogger.Object,
            _mockCrudRepository.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldPerformExpectedActions()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedId = fixture.Create<Id<TestEntity>>();
        var command = new DeleteEntityInput<TestEntity>(expectedId);
        var cts = new CancellationTokenSource();

        // Act
        var response = await _handler.Handle(command, cts.Token);

        // Assert
        response.Id.ShouldBe(expectedId);
        _mockCrudRepository.Verify(
            x => x.Delete(expectedId, cts.Token),
            Times.Once
        );
    }
}