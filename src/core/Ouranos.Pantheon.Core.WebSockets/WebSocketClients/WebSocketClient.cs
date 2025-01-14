using System.Net.WebSockets;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.Serializers;

namespace Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

public class WebSocketClient : IDisposable, IWebSocketClient
{
    private readonly uint _bufferSize;
    private readonly Uri _host;
    private readonly IReadOnlyCollection<IWebSocketInitializer> _initializers;
    private readonly IListenerRegistry _listenerRegistry;
    private readonly ILogger<WebSocketClient> _logger;
    private readonly IMessageSerializer _serializer;
    private readonly ClientWebSocket _webSocket = new();
    private Task? _listeningTask;

    public WebSocketClient(
        ILogger<WebSocketClient> logger,
        string host,
        uint bufferSize,
        IMessageSerializer serializer,
        IReadOnlyCollection<IWebSocketInitializer>? initializers,
        IListenerRegistry listenerRegistry
    )
    {
        ArgumentNullException.ThrowIfNull(logger);
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        ArgumentNullException.ThrowIfNull(serializer);
        ArgumentNullException.ThrowIfNull(listenerRegistry);

        _logger = logger;
        _host = new Uri(host);
        _listenerRegistry = listenerRegistry;
        _bufferSize = bufferSize;
        _initializers = initializers ?? [];
        _serializer = serializer;
    }

    public void Dispose()
    {
        _webSocket.Dispose();
    }

    public WebSocketState State => _webSocket.State;

    public async Task ConnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to connect to web socket.");
        cancellationToken.ThrowIfCancellationRequested();

        if (_webSocket.State is not WebSocketState.None)
        {
            const string errorMessage = "The web socket is already connected.";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        await _webSocket.ConnectAsync(_host, cancellationToken);
        await RunInitializers(cancellationToken);
        _listeningTask = Listen(cancellationToken);

        _logger.LogDebug("Connected to web socket.");
    }

    public async Task DisconnectAsync(CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to disconnect from web socket.");
        if (_webSocket.State != WebSocketState.Open)
        {
            _logger.LogDebug("Web socket was already disconnected.");
            return;
        }

        await _webSocket.CloseAsync(
            WebSocketCloseStatus.NormalClosure,
            "Client initiated close",
            cancellationToken
        );

        if (_listeningTask is not null)
        {
            await _listeningTask;
        }

        _logger.LogDebug("Successfully disconnected from web socket.");
    }

    public async Task SendAsync(byte[] message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to send message '{message}' bytes to web socket.", message);
        cancellationToken.ThrowIfCancellationRequested();

        if (_webSocket.State != WebSocketState.Open)
        {
            const string errorMessage = "WebSocket is not connected.";
            _logger.LogError(errorMessage);
            throw new InvalidOperationException(errorMessage);
        }

        await _webSocket.SendAsync(
            new ArraySegment<byte>(message),
            WebSocketMessageType.Text,
            true,
            cancellationToken
        );

        _logger.LogDebug("Successfully sent message bytes to web socket.");
    }

    public async Task SendAsync<T>(T message, CancellationToken cancellationToken = default)
    {
        _logger.LogTrace("Attempting to send message '{message}' to web socket.", message);
        cancellationToken.ThrowIfCancellationRequested();

        if (message is null)
        {
            return;
        }

        var messageBytes = _serializer.Serialize(message);
        await SendAsync(messageBytes, cancellationToken);

        _logger.LogDebug("Successfully sent message to web socket.");
    }

    private async Task Listen(CancellationToken cancellationToken)
    {
        var buffer = new byte[_bufferSize];

        try
        {
            while (_webSocket.State == WebSocketState.Open && !cancellationToken.IsCancellationRequested)
            {
                using var memoryStream = new MemoryStream();
                WebSocketReceiveResult receiveResult;

                do
                {
                    receiveResult = await _webSocket.ReceiveAsync(
                        new ArraySegment<byte>(buffer),
                        cancellationToken
                    );

                    if (receiveResult.MessageType == WebSocketMessageType.Close)
                    {
                        await HandleCloseMessage(cancellationToken);
                        return;
                    }

                    memoryStream.Write(buffer, 0, receiveResult.Count);
                } while (!receiveResult.EndOfMessage);

                var messageData = memoryStream.ToArray();
                await _listenerRegistry.HandleMessageAsync(messageData, this, cancellationToken);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred during receive message.");
            await HandleError(cancellationToken);
        }
    }

    private async Task HandleCloseMessage(CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            _logger.LogWarning("CLosing WebSocket connection due to a close message.");
            await _webSocket.CloseAsync(
                WebSocketCloseStatus.NormalClosure,
                "Closing connection",
                cancellationToken
            );
        }
    }

    private async Task HandleError(CancellationToken cancellationToken)
    {
        if (_webSocket.State == WebSocketState.Open)
        {
            _logger.LogError("Closing WebSocket connection due to an error.");
            await _webSocket.CloseAsync(
                WebSocketCloseStatus.InternalServerError,
                "Error occurred",
                cancellationToken
            );
        }
    }

    private async Task RunInitializers(CancellationToken cancellationToken)
    {
        var tasks = _initializers.Select(i => i.OnConnectedAsync(this, cancellationToken));
        await Task.WhenAll(tasks);
    }
}