using Ardalis.GuardClauses;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ouranos.Pantheon.Core.WebSockets.Listeners;
using Ouranos.Pantheon.Core.WebSockets.Serializers;
using Ouranos.Pantheon.Core.WebSockets.Serializers.TypeResolvers;

namespace Ouranos.Pantheon.Core.WebSockets.Builders;

public sealed class DiscriminatedRegistryBuilder : IDiscriminatedRegistryBuilder
{
    private readonly Dictionary<Type, IDiscriminatedMessagingBuilder> _messagingBuilders = [];
    private readonly IServiceCollection _services;

    private string? _discriminatorPath;

    public DiscriminatedRegistryBuilder(IServiceCollection services)
    {
        Guard.Against.Null(services);
        _services = services;
    }

    public IDiscriminatedRegistryBuilder UseDiscriminatorPath(string discriminatorPath)
    {
        _discriminatorPath = discriminatorPath;
        return this;
    }

    public IServiceCollection Build()
    {
        if (string.IsNullOrWhiteSpace(_discriminatorPath))
        {
            throw new InvalidOperationException("Discriminator path is not set.");
        }

        _services.TryAddTransient<ITypeResolver>(_ =>
            new JsonTypeResolver(
                _discriminatorPath,
                _messagingBuilders.ToDictionary(
                    x => x.Value.Discriminator,
                    x => x.Key
                )
            )
        );

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