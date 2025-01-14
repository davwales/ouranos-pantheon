using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.WebSockets.Listeners;

namespace Ouranos.Pantheon.Core.WebSockets.Builders;

public interface IConstantMessagingBuilder<out TMessage>
{
    IConstantMessagingBuilder<TMessage> UseListener<TListener>() where TListener : class, IListener<TMessage>;

    IServiceCollection Build();
}