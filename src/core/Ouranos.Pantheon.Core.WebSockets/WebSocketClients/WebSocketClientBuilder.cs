using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.WebSockets.Interfaces;

namespace Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

public sealed class WebSocketClientBuilder : IWebSocketClientBuilder
{
    private readonly List<Type> _listenerTypes = [];
    private readonly IServiceCollection _services;
    private uint _bufferSize = 4096;
    private string _host = string.Empty;

    public WebSocketClientBuilder(
        IServiceCollection services,
        WebSocketOptions? options = default
    )
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;

        if (options is null)
        {
            return;
        }

        ConfigureHost(options.Host);
        SetBuffer(options.BufferSize);
    }

    public IWebSocketClientBuilder ConfigureHost(string host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        _host = host;
        return this;
    }

    public IWebSocketClientBuilder SetBuffer(uint bufferSize)
    {
        _bufferSize = bufferSize;
        return this;
    }

    public IWebSocketClientBuilder AddListener<T>() where T : class, IListener
    {
        _listenerTypes.Add(typeof(T));
        _services.AddTransient<T>();
        return this;
    }

    public IServiceCollection Build()
    {
        return _services.AddSingleton<IWebSocketClient>(sp =>
            new WebSocketClient(
                sp.GetRequiredService<ILogger<WebSocketClient>>(),
                _host,
                _bufferSize,
                _listenerTypes.Select(t => (IListener)sp.GetRequiredService(t)).ToList()
            )
        );
    }
}