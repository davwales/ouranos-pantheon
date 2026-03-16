using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.Converters;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;

public sealed class WebSocketClientBuilder : IWebSocketClientBuilder
{
    private readonly IServiceCollection _services;

    public WebSocketClientBuilder(
        IServiceCollection services,
        WebSocketOptions? options = null
    )
    {
        Guard.Against.Null(services);
        _services = services;

        if (options is null)
        {
            return;
        }

        ConfigureHost(options.Host);
        UseBufferSize(options.BufferSize);
    }

    public uint BufferSize { get; private set; } = 4096;

    public Action? ConfigureMessaging { get; private set; }

    public string Host { get; private set; } = string.Empty;

    public IWebSocketClientBuilder ConfigureHost(string host)
    {
        Guard.Against.NullOrWhiteSpace(host);
        Host = host;
        return this;
    }

    public IWebSocketClientBuilder UseBufferSize(uint bufferSize)
    {
        BufferSize = bufferSize;
        return this;
    }

    public IWebSocketClientBuilder UseConstantMessage<TMessage>(
        Action<IConstantMessagingBuilder<TMessage>> configuration)
    {
        ConfigureMessaging = () =>
        {
            var messageBuilder = new ConstantMessagingBuilder<TMessage>(_services);
            configuration(messageBuilder);
            messageBuilder.Build();
        };

        return this;
    }

    public IWebSocketClientBuilder UseDiscriminatedMessages(Action<IDiscriminatedRegistryBuilder> configuration)
    {
        ConfigureMessaging = () =>
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
        ConfigureMessaging?.Invoke();

        UseTypeResolver<JsonTypeResolver>();
        UseConverter<JsonMessageConverter>();
        UseSerializer<MessageSerializer>();

        return _services
            .AddSingleton<IWebSocketClient>(sp =>
                new WebSocketClient(
                    sp.GetRequiredService<ILogger<WebSocketClient>>(),
                    Host,
                    BufferSize,
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