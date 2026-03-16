using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;

public interface IConstantMessagingBuilder<out TMessage>
{
    IConstantMessagingBuilder<TMessage> UseListener<TListener>() where TListener : class, IListener<TMessage>;

    IServiceCollection Build();
}