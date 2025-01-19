using MassTransit;
using Ouranos.Pantheon.Core.WebSockets;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Initializers;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Messages;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Serializers;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Worker;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Producer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .ConfigureWorker(builder.Configuration)
            .AddWebSockets(builder.Configuration, x => x
                .UseConverter<BsonMessageConverter>()
                .UseInitializer<SubscriptionInitializer>()
                .UseConstantMessage<SaleMessage>(m => m.UseListener<Listener>())
            )
            .AddApplicationModule()
            .AddXivApiModule(builder.Configuration)
            .AddPlutusDataLoaderRabbitMqModule(builder.Configuration, b => b
                .AddMediator(m => m.AddModuleConsumers())
            );

        return builder.Build();
    }
}