using MassTransit;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki;
using Ouranos.Pantheon.DataLoader.Plutus.Worker;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Producer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .ConfigureWorker(builder.Configuration)
            .AddHostedService<Worker>()
            .AddApplicationModule()
            .AddOsrsWikiModule(builder.Configuration)
            .AddPlutusDataLoaderRabbitMqModule(builder.Configuration, b => b
                .AddMediator(m => m.AddModuleConsumers())
            );

        return builder.Build();
    }
}