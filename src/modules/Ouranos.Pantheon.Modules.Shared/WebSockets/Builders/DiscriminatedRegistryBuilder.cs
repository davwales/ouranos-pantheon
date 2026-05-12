using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.TypeResolvers;

namespace Ouranos.Pantheon.Modules.Shared.WebSockets.Builders;

public sealed class DiscriminatedRegistryBuilder : IDiscriminatedRegistryBuilder
{
    private readonly Dictionary<Type, IDiscriminatedMessagingBuilder> _messagingBuilders = [];
    private readonly IServiceCollection _services;

    public DiscriminatedRegistryBuilder(IServiceCollection services)
    {
        Guard.Against.Null(services);
        _services = services;
    }

    public string? DiscriminatorPath { get; private set; }

    public IReadOnlyDictionary<Type, IDiscriminatedMessagingBuilder> MessagingBuilders =>
        _messagingBuilders;

    public IDiscriminatedRegistryBuilder UseDiscriminatorPath(string discriminatorPath)
    {
        Guard.Against.NullOrWhiteSpace(discriminatorPath);
        DiscriminatorPath = discriminatorPath;
        return this;
    }

    public IServiceCollection Build()
    {
        Guard.Against.Null(DiscriminatorPath);

        _services.TryAddTransient<ITypeResolver>(_ => new JsonTypeResolver(
            DiscriminatorPath,
            _messagingBuilders.ToDictionary(x => x.Value.Discriminator, x => x.Key)
        ));

        _services.TryAddSingleton<IListenerRegistry>(sp =>
        {
            var registry = new ListenerRegistry(sp.GetRequiredService<IMessageSerializer>());
            foreach (var (_, builder) in _messagingBuilders)
            {
                builder.RegisterListeners(registry, sp);
            }

            return registry;
        });

        return _services;
    }

    public IDiscriminatedRegistryBuilder UseMessage<TMessage>(
        string discriminatorValue,
        Action<DiscriminatedMessagingBuilder<TMessage>> configuration
    )
    {
        var type = typeof(TMessage);
        var builder = new DiscriminatedMessagingBuilder<TMessage>(discriminatorValue, _services);
        configuration(builder);
        _messagingBuilders[type] = builder;
        return this;
    }
}
