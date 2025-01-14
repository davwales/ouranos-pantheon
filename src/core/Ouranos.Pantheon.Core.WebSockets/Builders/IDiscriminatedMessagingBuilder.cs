using Ouranos.Pantheon.Core.WebSockets.Listeners;

namespace Ouranos.Pantheon.Core.WebSockets.Builders;

public interface IDiscriminatedMessagingBuilder
{
    string Discriminator { get; }

    void RegisterListeners(IListenerRegistry register, IServiceProvider sp);
}