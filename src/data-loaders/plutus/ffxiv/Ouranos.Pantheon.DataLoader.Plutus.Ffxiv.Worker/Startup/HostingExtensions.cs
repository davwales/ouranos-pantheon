using Ouranos.Pantheon.Core.WebSockets;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Application;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.Universalis;
using Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Infra.XivApi;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;

namespace Ouranos.Pantheon.DataLoader.Plutus.Ffxiv.Worker.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .AddWebSockets(builder.Configuration, x => x
                .AddListener<Listener>()
            )
            .AddApplicationModule()
            .AddUniversalisModule(builder.Configuration)
            .AddXivApiModule(builder.Configuration)
            .AddPlutusDataLoaderRabbitMqModule(builder.Configuration);

        return builder.Build();
    }
}