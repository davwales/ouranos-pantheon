using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders;

/// <summary>
/// Registers the data loaders (options, listeners, HTTP clients, WebSocket workers) and the
/// trade-message queue they publish to, honouring each loader's <c>IsEnabled</c> switch.
/// </summary>
internal static class DataLoaderModuleExtensions
{
    public static IHostApplicationBuilder AddDataLoaders(this IHostApplicationBuilder builder)
    {
        var dataLoadersSection = GetDataLoadersSection(builder.Configuration);

        builder
            .Services.Configure<DataLoadersOptions>(dataLoadersSection)
            .Configure<FfxivDataLoaderOptions>(
                dataLoadersSection.GetSection(FfxivDataLoaderOptions.SectionName)
            )
            .Configure<OsrsDataLoaderOptions>(
                dataLoadersSection.GetSection(OsrsDataLoaderOptions.SectionName)
            )
            .Configure<StocksDataLoaderOptions>(
                dataLoadersSection.GetSection(StocksDataLoaderOptions.SectionName)
            )
            .Configure<ConsumerDataLoaderOptions>(
                dataLoadersSection.GetSection(ConsumerDataLoaderOptions.SectionName)
            )
            .AddSingleton<IQueueTradeMessages, QueueTradeMessages>()
            .AddTransient<FfxivListener>()
            .AddTransient<FfxivSubscriptionInitializer>()
            .AddSingleton<IGetItems, GetItems>()
            .AddTransient<StocksTradeListener>()
            .AddTransient<StocksSuccessListener>()
            .AddTransient<StocksSubscriptionListener>()
            .AddTransient<StocksErrorListener>();

        builder.Services.AddHttpClient<IGithubClient, GithubClient>(
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<FfxivDataLoaderOptions>>();
                client.BaseAddress = new Uri(options.Value.XivApi.BaseAddress);
            }
        );

        builder.Services.AddHttpClient<IWikiClient, OsrsWikiClient>(
            (sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<OsrsDataLoaderOptions>>();
                client.BaseAddress = new Uri(options.Value.Wiki.BaseAddress);
            }
        );

        var loaders = ReadLoaders(dataLoadersSection);

        if (loaders.Ffxiv.IsEnabled)
        {
            builder.Services.AddHostedService(FfxivWebSocketWorkerFactory.Create);
        }

        if (loaders.Stocks.IsEnabled)
        {
            builder.Services.AddHostedService(StocksWebSocketWorkerFactory.Create);
        }

        return builder;
    }

    public static WolverineOptions ConfigureDataLoaderMessaging(
        this WolverineOptions opts,
        IConfiguration configuration
    )
    {
        var loaders = ReadLoaders(GetDataLoadersSection(configuration));

        if (
            !loaders.Ffxiv.IsEnabled
            && !loaders.Osrs.IsEnabled
            && !loaders.Stocks.IsEnabled
            && !loaders.Consumer.IsEnabled
        )
        {
            return opts;
        }

        opts.PublishMessage<TradeMessage>()
            .ToRabbitExchange(
                TradeMessage.Exchange,
                e =>
                {
                    e.BindQueue(TradeMessage.Queue);
                }
            );

        if (loaders.Consumer.IsEnabled)
        {
            opts.ListenToRabbitQueue(TradeMessage.Queue)
                .ListenerCount(4)
                .DeadLetterQueueing(new DeadLetterQueue(TradeMessage.DeadLetterQueue));
        }

        return opts;
    }

    private static IConfigurationSection GetDataLoadersSection(IConfiguration configuration)
    {
        return configuration
            .GetSection(PlutusOptions.SectionName)
            .GetSection(DataLoadersOptions.SectionName);
    }

    private static DataLoadersOptions ReadLoaders(IConfigurationSection dataLoadersSection)
    {
        return dataLoadersSection.Get<DataLoadersOptions>() ?? new DataLoadersOptions();
    }
}
