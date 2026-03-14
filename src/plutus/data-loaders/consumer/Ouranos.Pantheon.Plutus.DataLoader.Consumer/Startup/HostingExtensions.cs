using MassTransit;
using Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.InsertTrade;
using Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.UpsertSymbol;
using Ouranos.Pantheon.Plutus.DataLoader.Infra.RabbitMq;
using Ouranos.Pantheon.Plutus.DataLoader.Worker;
using Ouranos.Pantheon.Plutus.Service.Infra.Postgres;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .ConfigureWorker(builder.Configuration)
            .AddScoped<IUpsertSymbol, UpsertSymbol>()
            .AddScoped<IInsertTrade, InsertTrade>()
            .AddPostgresModule(builder.Configuration)
            .AddMassTransit(x =>
                {
                    x.AddConsumer<TradeConsumer>();
                    x.ConfigureRabbitMq(builder.Configuration);
                }
            );

        return builder.Build();
    }
}