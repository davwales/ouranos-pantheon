using MassTransit;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Application.Tests.Mediator;

public sealed class QueryHandlerTests
{
    private readonly TestQueryHandler _handler = new();

    [Fact]
    public async Task Consume_ShouldInvokeHandler()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedResult = fixture.Create<TestEntity>();
        var input = new TestQuery(expectedResult);
        var cts = new CancellationTokenSource();
        var context = new Mock<ConsumeContext<TestQuery>>();

        context.SetupGet(x => x.Message).Returns(input);
        context.Setup(x => x.CancellationToken).Returns(cts.Token);

        // Act
        await _handler.Consume(context.Object);

        // Assert
        _handler.HandleCount.ShouldBe(1);
        context.Verify(
            x => x.RespondAsync(expectedResult),
            Times.Once
        );
    }

    public sealed record TestQuery(TestEntity Result) : IQuery<TestEntity>;

    public sealed class TestQueryHandler : QueryHandler<TestQuery, TestEntity>
    {
        public int HandleCount;

        public override async Task<TestEntity> Handle(
            TestQuery query,
            CancellationToken cancellationToken = default
        )
        {
            HandleCount++;
            return await Task.FromResult(query.Result);
        }
    }
}