using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Queries.Common.GetEntity;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Application.Tests.Queries.Common.GetEntity;

public sealed class GetEntityHandlerTests
{
    private readonly GetEntityHandler<TestEntity> _handler;
    private readonly IRepository<TestEntity> _repository;

    public GetEntityHandlerTests()
    {
        _repository = Substitute.For<IRepository<TestEntity>>();

        _handler = new GetEntityHandler<TestEntity>(
            Substitute.For<ILogger<GetEntityHandler<TestEntity>>>(),
            _repository
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

        _repository.Read(expectedEntity.Id, cts.Token).Returns(expectedEntity);

        // Act
        var actualEntity = await _handler.Handle(input, cts.Token);

        // Assert
        actualEntity.ShouldBe(expectedEntity);
    }
}