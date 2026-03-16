using MassTransit;
using Ouranos.Pantheon.Modules.Shared.WebSockets;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Application;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Infra.XivApi;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Initializers;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Messages;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Serializers;
using Ouranos.Pantheon.Plutus.DataLoader.Infra.RabbitMq;
using Ouranos.Pantheon.Plutus.DataLoader.Worker;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Producer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .ConfigureWorker(builder.Configuration)
            .AddWebSockets(
                builder.Configuration,
                x => x
                    .UseConverter<BsonMessageConverter>()
                    .UseInitializer<SubscriptionInitializer>()
                    .UseConstantMessage<SaleMessage>(m => m.UseListener<Listener>())
            )
            .AddApplicationModule()
            .AddXivApiModule(builder.Configuration)
            .AddPlutusDataLoaderRabbitMqModule(
                builder.Configuration,
                b => b
                    .AddMediator(m => m.AddModuleConsumers())
            );

        return builder.Build();
    }
}