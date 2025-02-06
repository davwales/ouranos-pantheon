using MassTransit;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Core.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.InsertTrade;
using Ouranos.Pantheon.DataLoader.Plutus.Consumer.Handlers.UpsertSymbol;
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
            .AddCoreRabbitMqModule(
                builder.Configuration,
                x => x.AddConsumer<TradeConsumer>(),
                x => x.UseMessageRetry(c => c.Immediate(5))
            );

        return builder.Build();
    }
}