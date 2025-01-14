using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Listeners;

public sealed class ListenerDispatcher<T> : IListenerDispatcher
{
    private readonly IListener<T> _listener;

    public ListenerDispatcher(IListener<T> listener)
    {
        ArgumentNullException.ThrowIfNull(listener);
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