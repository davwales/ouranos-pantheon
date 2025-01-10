using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.RabbitMq.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.RabbitMq;

public static class RabbitMqModule
{
    public static IServiceCollection AddRabbitMqModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddCoreRabbitMqModule(configuration)
            .AddSingleton<IQueueTradeMessage, QueueTradeMessage>();
    }
}