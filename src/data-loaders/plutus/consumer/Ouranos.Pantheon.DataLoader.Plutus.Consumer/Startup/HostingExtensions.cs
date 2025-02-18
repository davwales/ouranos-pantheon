using MassTransit;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.UpsertSymbol;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Worker;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .ConfigureWorker(builder.Configuration)
            .AddScoped<IUpsertSymbol, UpsertSymbol>()
            .AddScoped<IInsertTrade, InsertTrade>()
            .AddCoreMongo(builder.Configuration)
            .AddMassTransit(x =>
            {
                x.AddConsumer<TradeConsumer>();
                x.ConfigureRabbitMq(builder.Configuration);
            });

        return builder.Build();
    }
}