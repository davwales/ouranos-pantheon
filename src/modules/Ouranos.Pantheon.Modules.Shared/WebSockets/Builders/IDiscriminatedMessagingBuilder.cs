using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;

public interface IDiscriminatedMessagingBuilder
{
    string Discriminator { get; }

    void RegisterListeners(IListenerRegistry register, IServiceProvider sp);
}