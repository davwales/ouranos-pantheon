using MassTransit;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Mediator;

public class CommandHandlerTests
{
    private readonly TestCommandHandler _handler = new();

    [Fact]
    public async Task Consume_ShouldInvokeHandler()
    {
        // Arrange
        var input = new TestCommand();
        var cts = new CancellationTokenSource();
        var context = Substitute.For<ConsumeContext<TestCommand>>();

        context.Message.Returns(input);
        context.CancellationToken.Returns(cts.Token);

        // Act
        await _handler.Consume(context);

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