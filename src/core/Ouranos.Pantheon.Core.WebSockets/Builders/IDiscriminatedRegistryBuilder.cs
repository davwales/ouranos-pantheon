using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Core.WebSockets.Builders;

public interface IDiscriminatedRegistryBuilder
{
    IDiscriminatedRegistryBuilder UseDiscriminatorPath(string discriminatorPath);

    IDiscriminatedRegistryBuilder UseMessage<TMessage>(
        string discriminatorValue,
        Action<DiscriminatedMessagingBuilder<TMessage>> configuration
    );

    IServiceCollection Build();
}