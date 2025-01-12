using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.WebSockets.Interfaces;

namespace Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

public interface IWebSocketClientBuilder
{
    IWebSocketClientBuilder ConfigureHost(string host);

    IWebSocketClientBuilder SetBuffer(uint bufferSize);

    IWebSocketClientBuilder AddListener<T>() where T : class, IListener;

    IServiceCollection Build();
}