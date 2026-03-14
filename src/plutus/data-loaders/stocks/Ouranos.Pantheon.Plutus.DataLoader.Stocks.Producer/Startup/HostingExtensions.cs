using Ouranos.Pantheon.Core.WebSockets;
using Ouranos.Pantheon.Plutus.DataLoader.Infra.RabbitMq;
using Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Listeners;
using Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Messages;
using Ouranos.Pantheon.Plutus.DataLoader.Worker;

namespace Ouranos.Pantheon.Plutus.DataLoader.Stocks.Producer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .ConfigureWorker(builder.Configuration)
            .Configure<AlpacaOptions>(builder.Configuration.GetSection(AlpacaOptions.SectionName))
            .AddWebSockets(
                builder.Configuration,
                x => x
                    .UseDiscriminatedMessages(d => d
                        .UseDiscriminatorPath("T")
                        .UseMessage<ErrorMessage>(ErrorMessage.TypeIndicator, m => m.UseListener<ErrorListener>())
                        .UseMessage<SuccessMessage>(SuccessMessage.TypeIndicator, m => m.UseListener<SuccessListener>())
                        .UseMessage<SubscriptionAckMessage>(
                            SubscriptionAckMessage.TypeIndicator,
                            m => m.UseListener<SubscriptionListener>()
                        )
                        .UseMessage<AlpacaTradeMessage>(
                            AlpacaTradeMessage.TypeIndicator,
                            m => m.UseListener<TradeListener>()
                        )
                    )
            )
            .AddPlutusDataLoaderRabbitMqModule(builder.Configuration);

        return builder.Build();
    }
}