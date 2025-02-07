using MassTransit;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Core.Application.Tests.Mediator;

public sealed class CommandHandlerWithResultTests
{
    private readonly TestCommandHandler _handler = new();

    [Fact]
    public async Task Consume_ShouldInvokeHandler()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedResult = fixture.Create<TestEntity>();
        var input = new TestCommand(expectedResult);
        var cts = new CancellationTokenSource();
        var context = Substitute.For<ConsumeContext<TestCommand>>();

        context.Message.Returns(input);
        context.CancellationToken.Returns(cts.Token);

        // Act
        await _handler.Consume(context);

        // Assert
        _handler.HandleCount.ShouldBe(1);
        await context.Received(1).RespondAsync(expectedResult);
    }

    public sealed record TestCommand(TestEntity Result) : ICommand<TestEntity>;

    public sealed class TestCommandHandler : CommandHandler<TestCommand, TestEntity>
    {
        public int HandleCount;

        public override async Task<TestEntity> Handle(
            TestCommand command,
            CancellationToken cancellationToken = default
        )
        {
            HandleCount++;
            return await Task.FromResult(command.Result);
        }
    }
}