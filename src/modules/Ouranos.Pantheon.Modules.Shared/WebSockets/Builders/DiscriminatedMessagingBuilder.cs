using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;

public sealed class DiscriminatedMessagingBuilder<TMessage> : IDiscriminatedMessagingBuilder
{
    private readonly List<Func<IServiceProvider, IListener<TMessage>>> _getListeners = [];
    private readonly IServiceCollection _services;

    public DiscriminatedMessagingBuilder(string discriminator, IServiceCollection services)
    {
        Guard.Against.NullOrWhiteSpace(discriminator);
        Guard.Against.Null(services);

        Discriminator = discriminator;
        _services = services;
    }

    public string Discriminator { get; init; }

    public void RegisterListeners(IListenerRegistry registry, IServiceProvider sp)
    {
        foreach (var listener in _getListeners.Select(getListener => getListener(sp)))
        {
            registry.RegisterListener(listener);
        }
    }

    public DiscriminatedMessagingBuilder<TMessage> UseListener<TListener>()
        where TListener : class, IListener<TMessage>
    {
        _services.TryAddTransient<TListener>();
        _getListeners.Add(sp => sp.GetRequiredService<TListener>());
        return this;
    }
}
