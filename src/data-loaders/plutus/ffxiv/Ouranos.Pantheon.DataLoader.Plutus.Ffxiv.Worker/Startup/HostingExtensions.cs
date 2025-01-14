using Ouranos.Pantheon.Core.WebSockets;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Initializers;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Messages;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Serializers;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .AddWebSockets(builder.Configuration, x => x
                .UseConverter<BsonMessageConverter>()
                .UseInitializer<SubscriptionInitializer>()
                .UseConstantMessage<SaleMessage>(m => m.UseListener<Listener>())
            )
            .AddApplicationModule()
            .AddXivApiModule(builder.Configuration)
            .AddPlutusDataLoaderRabbitMqModule(builder.Configuration);

        return builder.Build();
    }
}