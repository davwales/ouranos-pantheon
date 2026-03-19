using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Mediator;

public sealed class QueryHandlerTests
{
    private readonly TestQueryHandler _handler = new();

    [Fact]
    public async Task Handle_ShouldInvokeHandlerAndReturnResult()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedResult = fixture.Create<TestEntity>();
        var input = new TestQuery(expectedResult);
        var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.Handle(input, cts.Token);

        // Assert
        _handler.HandleCount.ShouldBe(1);
        result.ShouldBe(expectedResult);
    }

    public sealed record TestQuery(TestEntity Result);

    public sealed class TestQueryHandler : IPantheonHandler<TestQuery, TestEntity>
    {
        public int HandleCount;

        public async Task<TestEntity> Handle(
            TestQuery query,
            CancellationToken cancellationToken = default
        )
        {
            HandleCount++;
            return await Task.FromResult(query.Result);
        }
    }
}
