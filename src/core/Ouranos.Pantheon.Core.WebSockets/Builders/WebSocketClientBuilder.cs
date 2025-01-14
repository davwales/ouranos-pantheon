using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.Serializers;
using Ouranos.Pantheon.Core.WebSockets.Serializers.Converters;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Builders;

public sealed class WebSocketClientBuilder : IWebSocketClientBuilder
{
    private readonly IServiceCollection _services;
    private uint _bufferSize = 4096;
    private Action? _configureMessaging;
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
        UseBufferSize(options.BufferSize);
    }

    public IWebSocketClientBuilder ConfigureHost(string host)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(host);
        _host = host;
        return this;
    }

    public IWebSocketClientBuilder UseBufferSize(uint bufferSize)
    {
        _bufferSize = bufferSize;
        return this;
    }

    public IWebSocketClientBuilder UseConstantMessage<TMessage>(
        Action<IConstantMessagingBuilder<TMessage>> configuration)
    {
        _configureMessaging = () =>
        {
            var messageBuilder = new ConstantMessagingBuilder<TMessage>(_services);
            configuration(messageBuilder);
            messageBuilder.Build();
        };

        return this;
    }

    public IWebSocketClientBuilder UseDiscriminatedMessages(Action<IDiscriminatedRegistryBuilder> configuration)
    {
        _configureMessaging = () =>
        {
            var messageBuilder = new DiscriminatedRegistryBuilder(_services);
            configuration(messageBuilder);
            messageBuilder.Build();
        };

        return this;
    }

    public IWebSocketClientBuilder UseSerializer<T>() where T : class, IMessageSerializer
    {
        _services.TryAddSingleton<IMessageSerializer, T>();
        return this;
    }

    public IWebSocketClientBuilder UseInitializer<T>() where T : class, IWebSocketInitializer
    {
        _services.AddTransient<IWebSocketInitializer, T>();
        return this;
    }

    public IServiceCollection Build()
    {
        _configureMessaging?.Invoke();
        _services.TryAddSingleton<IMessageConverter, JsonMessageConverter>();
        _services.TryAddSingleton<IMessageConverter, JsonMessageConverter>();
        _services.TryAddSingleton<IMessageSerializer, MessageSerializer>();

        return _services
            .AddSingleton<IWebSocketClient>(sp =>
                new WebSocketClient(
                    sp.GetRequiredService<ILogger<WebSocketClient>>(),
                    _host,
                    _bufferSize,
                    sp.GetRequiredService<IMessageSerializer>(),
                    sp.GetServices<IWebSocketInitializer>().ToList(),
                    sp.GetRequiredService<IListenerRegistry>()
                )
            );
    }

    public IWebSocketClientBuilder UseConverter<T>() where T : class, IMessageConverter
    {
        _services.TryAddSingleton<IMessageConverter, T>();
        return this;
    }

    public IWebSocketClientBuilder UseTypeResolver<T>() where T : class, ITypeResolver
    {
        _services.TryAddSingleton<ITypeResolver, T>();
        return this;
    }
}