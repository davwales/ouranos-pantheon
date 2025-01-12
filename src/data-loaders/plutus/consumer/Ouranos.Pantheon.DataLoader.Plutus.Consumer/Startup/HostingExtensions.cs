using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Core.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .AddApplicationModule()
            .AddCoreMongo(builder.Configuration)
            .AddCoreRabbitMqModule(builder.Configuration,
                configurator => configurator.AddConsumer<TradeConsumer>());

        return builder.Build();
    }
}