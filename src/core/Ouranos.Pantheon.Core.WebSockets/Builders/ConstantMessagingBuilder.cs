using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.Serializers;
using Ouranos.Pantheon.Core.WebSockets.Serializers.TypeResolvers;

namespace Ouranos.Pantheon.Core.WebSockets.Builders;

public sealed class ConstantMessagingBuilder<TMessage> : IConstantMessagingBuilder<TMessage>
{
    private readonly List<Func<IServiceProvider, IListener<TMessage>>> _getListeners = [];
    private readonly IServiceCollection _services;

    public ConstantMessagingBuilder(IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _services = services;
    }

    public IConstantMessagingBuilder<TMessage> UseListener<TListener>() where TListener : class, IListener<TMessage>
    {
        _services.TryAddTransient<TListener>();
        _getListeners.Add(sp => sp.GetRequiredService<TListener>());
        return this;
    }

    public IServiceCollection Build()
    {
        _services.TryAddTransient<ITypeResolver>(_ => new ConstantTypeResolver(typeof(TMessage)));
        _services.TryAddSingleton<IListenerRegistry>(sp =>
        {
            var registry = new ListenerRegistry(sp.GetRequiredService<IMessageSerializer>());
            foreach (var listener in _getListeners.Select(getListener => getListener(sp)))
            {
                registry.RegisterListener(listener);
            }

            return registry;
        });

        return _services;
    }
}