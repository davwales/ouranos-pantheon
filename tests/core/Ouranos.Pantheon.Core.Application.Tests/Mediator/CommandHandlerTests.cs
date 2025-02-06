using MassTransit;
using Ouranos.Pantheon.Core.Application.Mediator;

namespace Ouranos.Pantheon.Core.Application.Tests.Mediator;

public class CommandHandlerTests
{
    private readonly TestCommandHandler _handler = new();

    [Fact]
    public async Task Consume_ShouldInvokeHandler()
    {
        // Arrange
        var input = new TestCommand();
        var cts = new CancellationTokenSource();
        var context = new Mock<ConsumeContext<TestCommand>>();

        context.SetupGet(x => x.Message).Returns(input);
        context.Setup(x => x.CancellationToken).Returns(cts.Token);

        // Act
        await _handler.Consume(context.Object);

        // Assert
        _handler.HandleCount.ShouldBe(1);
    }

    public sealed record TestCommand : ICommand;

    public sealed class TestCommandHandler : CommandHandler<TestCommand>
    {
        public int HandleCount;

        public override async Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            HandleCount++;
            await Task.CompletedTask;
        }
    }
}