using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.TypeResolvers;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;

public sealed class ConstantMessagingBuilder<TMessage> : IConstantMessagingBuilder<TMessage>
{
    private readonly List<Func<IServiceProvider, IListener<TMessage>>> _getListeners = [];
    private readonly IServiceCollection _services;

    public ConstantMessagingBuilder(IServiceCollection services)
    {
        Guard.Against.Null(services);
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