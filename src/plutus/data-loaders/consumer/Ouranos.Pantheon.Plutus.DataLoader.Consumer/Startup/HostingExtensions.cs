using MassTransit;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Plutus.DataLoader.Infra.RabbitMq;
using Ouranos.Pantheon.Plutus.DataLoader.Worker;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .ConfigureWorker(builder.Configuration)
            .AddCorePostgresModule<PlutusDbContext>(builder.Configuration)
            .AddMassTransit(x =>
                {
                    x.AddConsumer<TradeConsumer>();
                    x.ConfigureRabbitMq(builder.Configuration);
                }
            );

        return builder.Build();
    }
}