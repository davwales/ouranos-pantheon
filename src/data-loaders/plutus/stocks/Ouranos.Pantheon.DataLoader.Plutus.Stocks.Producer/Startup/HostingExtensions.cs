using Ouranos.Pantheon.Core.WebSockets;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Listeners;
using Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Messages;

namespace Ouranos.Pantheon.DataLoader.Plutus.Stocks.Producer.Startup;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this HostApplicationBuilder builder)
    {
        builder.Services
            .Configure<AlpacaOptions>(builder.Configuration.GetSection(AlpacaOptions.SectionName))
            .AddWebSockets(builder.Configuration, x => x
                .UseDiscriminatedMessages(d => d
                    .UseDiscriminatorPath("T")
                    .UseMessage<ErrorMessage>(ErrorMessage.TypeIndicator, m => m.UseListener<ErrorListener>())
                    .UseMessage<SuccessMessage>(SuccessMessage.TypeIndicator, m => m.UseListener<SuccessListener>())
                    .UseMessage<SubscriptionAckMessage>(SubscriptionAckMessage.TypeIndicator,
                        m => m.UseListener<SubscriptionListener>())
                    .UseMessage<AlpacaTradeMessage>(AlpacaTradeMessage.TypeIndicator,
                        m => m.UseListener<TradeListener>())
                )
            )
            .AddPlutusDataLoaderRabbitMqModule(builder.Configuration);

        return builder.Build();
    }
}