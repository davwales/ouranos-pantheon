using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Tests.Utils;

namespace Ouranos.Pantheon.Modules.Shared.Tests.Application.Mediator;

public sealed class CommandHandlerWithResultTests
{
    private readonly TestCommandHandler _handler = new();

    [Fact]
    public async Task Handle_ShouldInvokeHandlerAndReturnResult()
    {
        // Arrange
        var fixture = new Fixture();
        var expectedResult = fixture.Create<TestEntity>();
        var input = new TestCommand(expectedResult);
        var cts = new CancellationTokenSource();

        // Act
        var result = await _handler.Handle(input, cts.Token);

        // Assert
        _handler.HandleCount.ShouldBe(1);
        result.ShouldBe(expectedResult);
    }

    public sealed record TestCommand(TestEntity Result);

    public sealed class TestCommandHandler : IPantheonHandler<TestCommand, TestEntity>
    {
        public int HandleCount;

        public async Task<TestEntity> Handle(
            TestCommand command,
            CancellationToken cancellationToken = default
        )
        {
            HandleCount++;
            return await Task.FromResult(command.Result);
        }
    }
}
