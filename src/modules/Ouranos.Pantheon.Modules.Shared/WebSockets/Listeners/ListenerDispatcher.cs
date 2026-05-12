using Ardalis.GuardClauses;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;

public sealed record ListenerDispatcher<T> : IListenerDispatcher
{
    private readonly IListener<T> _listener;

    public ListenerDispatcher(IListener<T> listener)
    {
        Guard.Against.Null(listener);
        _listener = listener;
    }

    public async Task HandleMessageAsync(
        object message,
        IWebSocketClient client,
        CancellationToken cancellationToken = default
    )
    {
        if (message is not T typedMessage)
        {
            return;
        }

        await _listener.HandleMessageAsync(typedMessage, client, cancellationToken);
    }
}
