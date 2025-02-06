using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetAllEntities;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Application.Tests.Queries.Common.GetAllEntities;

public sealed class GetAllEntitiesHandlerTests
{
    private readonly GetAllEntitiesHandler<TestEntity> _handler;
    private readonly Mock<ICrudRepository<TestEntity>> _mockCrudRepository = new();
    private readonly Mock<ILogger<GetAllEntitiesHandler<TestEntity>>> _mockLogger = new();

    public GetAllEntitiesHandlerTests()
    {
        _handler = new GetAllEntitiesHandler<TestEntity>(
            _mockLogger.Object,
            _mockCrudRepository.Object
        );
    }

    [Fact]
    public async Task Handle_ShouldReturnExpectedResults()
    {
        // Arrange
        var fixture = new Fixture();
        var input = new GetAllEntitiesInput<TestEntity>();
        var expectedQuery = fixture.CreateMany<TestEntity>().AsQueryable();
        var cts = new CancellationTokenSource();

        _mockCrudRepository
            .Setup(x => x.AsQueryable(cts.Token))
            .Returns(expectedQuery);

        // Act
        var response = await _handler.Handle(input, cts.Token);

        // Assert
        response.Value.ShouldBe(expectedQuery);
    }
}