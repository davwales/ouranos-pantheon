using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Application.Tests.Queries.Common.GetEntity;

public sealed class GetEntityHandlerTests
{
    private readonly GetEntityHandler<TestEntity> _handler;
    private readonly Mock<ICrudRepository<TestEntity>> _mockCrudRepository = new();
    private readonly Mock<ILogger<GetEntityHandler<TestEntity>>> _mockLogger = new();

    public GetEntityHandlerTests()
    {
        _handler = new GetEntityHandler<TestEntity>(
            _mockLogger.Object,
            _mockCrudRepository.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnExpectedResults()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedEntity = fixture.Create<TestEntity>();
        var input = new GetEntityInput<TestEntity>(expectedEntity.Id);
        var cts = new CancellationTokenSource();

        _mockCrudRepository
            .Setup(x => x.Read(expectedEntity.Id, cts.Token))
            .ReturnsAsync(expectedEntity);

        // Act
        var actualEntity = await _handler.Handle(input, cts.Token);

        // Assert
        actualEntity.ShouldBe(expectedEntity);
    }
}