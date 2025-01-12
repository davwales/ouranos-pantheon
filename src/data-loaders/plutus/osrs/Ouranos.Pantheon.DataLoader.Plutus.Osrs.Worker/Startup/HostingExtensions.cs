using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application;
using Ouranos.Pantheon.DataLoader.Plutus.Osrs.Infra.OsrsWiki;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Worker.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .AddHostedService<Worker>()
            .AddApplicationModule()
            .AddOsrsWikiModule(builder.Configuration)
            .AddPlutusDataLoaderRabbitMqModule(builder.Configuration);

        return builder.Build();
    }
}