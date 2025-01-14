using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.WebSockets.Serializers;
using Ouranos.Pantheon.Core.WebSockets.WebSocketClients;

namespace Ouranos.Pantheon.Core.WebSockets.Builders;

public interface IWebSocketClientBuilder
{
    IWebSocketClientBuilder ConfigureHost(string host);

    IWebSocketClientBuilder UseBufferSize(uint bufferSize);

    IWebSocketClientBuilder UseConstantMessage<TMessage>(Action<IConstantMessagingBuilder<TMessage>> configuration);

    IWebSocketClientBuilder UseDiscriminatedMessages(Action<IDiscriminatedRegistryBuilder> configuration);

    IWebSocketClientBuilder UseSerializer<T>() where T : class, IMessageSerializer;

    IWebSocketClientBuilder UseConverter<T>() where T : class, IMessageConverter;

    IWebSocketClientBuilder UseTypeResolver<T>() where T : class, ITypeResolver;

    IWebSocketClientBuilder UseInitializer<T>() where T : class, IWebSocketInitializer;

    IServiceCollection Build();
}