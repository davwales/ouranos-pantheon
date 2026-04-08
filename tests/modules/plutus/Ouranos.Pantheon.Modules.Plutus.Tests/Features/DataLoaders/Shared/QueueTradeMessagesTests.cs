using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Wolverine.Runtime;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.DataLoaders.Shared;

public sealed class QueueTradeMessagesTests
{
    private readonly ILogger<QueueTradeMessages> _logger = Substitute.For<ILogger<QueueTradeMessages>>();
    private readonly IWolverineRuntime _wolverineRuntime = Substitute.For<IWolverineRuntime>();
    private readonly QueueTradeMessages _queue;

    public QueueTradeMessagesTests()
    {
        _queue = new QueueTradeMessages(_logger, _wolverineRuntime);
    }

    [Fact]
    public async Task QueueMessages_WhenEmptyCollection_ShouldNotInteractWithRuntime()
    {
        // Act
        await _queue.QueueMessages([], CancellationToken.None);

        // Assert
        _wolverineRuntime.ReceivedCalls().ShouldBeEmpty();
    }

    [Fact]
    public async Task QueueMessages_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var ct = new CancellationToken(true);
        var messages = new List<TradeMessage>
        {
            new(Producer.Osrs, "1234", null, "Test Item", 100m, 5m, DateTimeOffset.UtcNow, new AdditionalFields())
        };

        // Act
        var act = async () => await _queue.QueueMessages(messages, ct);

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }
}
