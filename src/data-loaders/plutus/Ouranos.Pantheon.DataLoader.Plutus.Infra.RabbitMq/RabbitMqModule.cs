using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;

public static class RabbitMqModule
{
    public static IServiceCollection AddPlutusDataLoaderRabbitMqModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddCoreRabbitMqModule(configuration)
            .AddSingleton<IQueueTradeMessages, QueueTradeMessages>();
    }
}