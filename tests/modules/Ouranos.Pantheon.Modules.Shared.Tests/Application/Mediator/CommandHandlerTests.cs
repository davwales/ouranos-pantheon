using Ouranos.Pantheon.Modules.Shared.Application;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Mediator;

public class CommandHandlerTests
{
    private readonly TestCommandHandler _handler = new();

    [Fact]
    public async Task Handle_ShouldInvokeHandler()
    {
        // Arrange
        var input = new TestCommand();
        var cts = new CancellationTokenSource();

        // Act
        await _handler.Handle(input, cts.Token);

        // Assert
        _handler.HandleCount.ShouldBe(1);
    }

    public sealed record TestCommand;

    public sealed class TestCommandHandler : IPantheonHandler<TestCommand>
    {
        public int HandleCount;

        public async Task Handle(TestCommand command, CancellationToken cancellationToken = default)
        {
            HandleCount++;
            await Task.CompletedTask;
        }
    }
}
