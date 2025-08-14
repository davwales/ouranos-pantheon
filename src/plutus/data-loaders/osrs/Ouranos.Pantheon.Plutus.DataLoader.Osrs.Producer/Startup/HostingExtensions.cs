using MassTransit;
using Ouranos.Pantheon.Plutus.DataLoader.Infra.RabbitMq;
using Ouranos.Pantheon.Plutus.DataLoader.Osrs.Application;
using Ouranos.Pantheon.Plutus.DataLoader.Osrs.Infra.OsrsWiki;
using Ouranos.Pantheon.Plutus.DataLoader.Worker;

namespace Ouranos.Pantheon.Plutus.DataLoader.Osrs.Producer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .ConfigureWorker(builder.Configuration)
            .AddHostedService<Worker>()
            .AddApplicationModule()
            .AddOsrsWikiModule(builder.Configuration)
            .AddPlutusDataLoaderRabbitMqModule(
                builder.Configuration,
                b => b
                    .AddMediator(m => m.AddModuleConsumers())
            );

        return builder.Build();
    }
}