using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Infra.RabbitMq;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .AddApplicationModule()
            .AddCoreMongo(builder.Configuration)
            .AddRabbitMqModule(builder.Configuration,
                configurator => configurator.AddConsumer<TradeConsumer>());

        return builder.Build();
    }
}