using MassTransit;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Shared.WebSockets;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Listeners;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.Converters;
using Ouranos.Pantheon.Modules.Shared.WebSockets.Serializers.TypeResolvers;
using Ouranos.Pantheon.Modules.Shared.WebSockets.WebSocketClients;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Messages;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.Serializers;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Ffxiv.XivApi;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Osrs;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Stocks.Messages;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders.Consumer;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe;

namespace Ouranos.Pantheon.Modules.Plutus;

public sealed class PlutusModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddCoreOuranosMachineLearningModule(builder.Configuration)
            .AddCorePostgresModule<PlutusDbContext>(
                builder.Configuration,
                typeof(PlutusModule).Assembly
            );

        ConfigureDataLoaders(builder);
        return builder;
    }

    public async Task<IHost> Configure(IHost host)
    {
        await host.Services.ApplyCorePostgresMigrations<PlutusDbContext>();
        return host;
    }

    public void MapEndpoints(WebApplication app)
    {
        GetAllMarketsEndpoint.Map(app);
        GetMarketEndpoint.Map(app);
        CreateMarketEndpoint.Map(app);
        UpdateMarketEndpoint.Map(app);
        DeleteMarketEndpoint.Map(app);
        GetMarketTradesEndpoint.Map(app);
        GetAllTradesEndpoint.Map(app);
        GetRecipeTradesEndpoint.Map(app);
        GetSymbolTradesEndpoint.Map(app);
        GetAllSymbolsEndpoint.Map(app);
        GetSymbolEndpoint.Map(app);
        GetDailySymbolSummaryEndpoint.Map(app);
        GetAllForecastsEndpoint.Map(app);
        GetMarketForecastEndpoint.Map(app);
        GetAllRecipesEndpoint.Map(app);
        GetRecipeEndpoint.Map(app);
        CreateRecipeEndpoint.Map(app);
        UpdateRecipeEndpoint.Map(app);
        DeleteRecipeEndpoint.Map(app);
    }

    private static void ConfigureDataLoaders(IHostApplicationBuilder builder)
    {
        var plutusOptionsSection = builder.Configuration.GetSection(PlutusOptions.SectionName);
        var dataLoadersSection = plutusOptionsSection.GetSection(DataLoadersOptions.SectionName);

        builder.Services
            .Configure<PlutusOptions>(plutusOptionsSection)
            .Configure<DataLoadersOptions>(dataLoadersSection)
            .Configure<FfxivDataLoaderOptions>(dataLoadersSection.GetSection(FfxivDataLoaderOptions.SectionName))
            .Configure<OsrsDataLoaderOptions>(dataLoadersSection.GetSection(OsrsDataLoaderOptions.SectionName))
            .Configure<StocksDataLoaderOptions>(dataLoadersSection.GetSection(StocksDataLoaderOptions.SectionName))
            .Configure<ConsumerDataLoaderOptions>(dataLoadersSection.GetSection(ConsumerDataLoaderOptions.SectionName));

        var loaders = dataLoadersSection.Get<DataLoadersOptions>() ?? new DataLoadersOptions();
        var areDataLoadersEnabled = loaders.Ffxiv.IsEnabled || loaders.Osrs.IsEnabled || loaders.Stocks.IsEnabled ||
                                    loaders.Consumer.IsEnabled;
        if (areDataLoadersEnabled)
        {
            builder.Services
                .AddSingleton<IQueueTradeMessages, QueueTradeMessages>()
                .AddMassTransit<IPlutusDataBus>(x =>
                    {
                        if (loaders.Consumer.IsEnabled)
                        {
                            x.AddConsumer<TradeConsumer>();
                        }

                        x.UsingRabbitMq((ctx, cfg) =>
                            {
                                var options = loaders.RabbitMq;

                                cfg.Host(
                                    options.Host,
                                    "/",
                                    h =>
                                    {
                                        h.Username(options.Username);
                                        h.Password(options.Password);
                                    }
                                );

                                if (options.ConcurrencyLimit.HasValue)
                                {
                                    cfg.UseConcurrencyLimit(options.ConcurrencyLimit.Value);
                                }

                                if (options.RetryCount.HasValue)
                                {
                                    cfg.UseMessageRetry(r => r.Immediate(options.RetryCount.Value));
                                }

                                cfg.ConfigureEndpoints(ctx);
                            }
                        );
                    }
                );
        }

        if (loaders.Ffxiv.IsEnabled)
        {
            builder.Services
                .AddTransient<FfxivListener>()
                .AddTransient<FfxivSubscriptionInitializer>()
                .AddMemoryCache()
                .AddSingleton<IGetItems, GetItems>()
                .AddHttpClient<IGithubClient, GithubClient>((sp, client) =>
                    {
                        var options = sp.GetRequiredService<IOptions<FfxivDataLoaderOptions>>();
                        client.BaseAddress = new Uri(options.Value.XivApi.BaseAddress);
                    }
                );

            builder.Services.AddHostedService(BuildFfxivWorker);
        }

        if (loaders.Osrs.IsEnabled)
        {
            builder.Services
                .AddHttpClient<IWikiClient, OsrsWikiClient>((sp, client) =>
                    {
                        var options = sp.GetRequiredService<IOptions<OsrsDataLoaderOptions>>();
                        client.BaseAddress = new Uri(options.Value.Wiki.BaseAddress);
                    }
                );

            builder.Services.AddHostedService<OsrsWorker>();
        }

        if (loaders.Stocks.IsEnabled)
        {
            builder.Services
                .AddTransient<StocksTradeListener>()
                .AddTransient<StocksSuccessListener>()
                .AddTransient<StocksSubscriptionListener>()
                .AddTransient<StocksErrorListener>();

            builder.Services.AddHostedService(BuildStocksWorker);
        }
    }

    private static WebSocketWorker BuildFfxivWorker(IServiceProvider sp)
    {
        var wsOptions = sp.GetRequiredService<IOptions<FfxivDataLoaderOptions>>().Value.WebSocket;

        var converter = new BsonMessageConverter();
        var typeResolver = new ConstantTypeResolver(typeof(SaleMessage));
        var serializer = new MessageSerializer(typeResolver, converter);
        var registry = new ListenerRegistry(serializer);

        var listener = sp.GetRequiredService<FfxivListener>();
        registry.RegisterListener(listener);

        var initializer = sp.GetRequiredService<FfxivSubscriptionInitializer>();

        var client = new WebSocketClient(
            sp.GetRequiredService<ILogger<WebSocketClient>>(),
            wsOptions.Host,
            wsOptions.BufferSize,
            serializer,
            [initializer],
            registry
        );

        return new WebSocketWorker(
            sp.GetRequiredService<ILogger<WebSocketWorker>>(),
            client,
            Options.Create(wsOptions)
        );
    }

    private static WebSocketWorker BuildStocksWorker(IServiceProvider sp)
    {
        var wsOptions = sp.GetRequiredService<IOptions<StocksDataLoaderOptions>>().Value.WebSocket;

        var typeMap = new Dictionary<string, Type>
        {
            [ErrorMessage.TypeIndicator] = typeof(ErrorMessage),
            [SuccessMessage.TypeIndicator] = typeof(SuccessMessage),
            [SubscriptionAckMessage.TypeIndicator] = typeof(SubscriptionAckMessage),
            [AlpacaTradeMessage.TypeIndicator] = typeof(AlpacaTradeMessage)
        };

        var converter = new JsonMessageConverter();
        var typeResolver = new JsonTypeResolver("T", typeMap);
        var serializer = new MessageSerializer(typeResolver, converter);
        var registry = new ListenerRegistry(serializer);

        registry.RegisterListener(sp.GetRequiredService<StocksErrorListener>());
        registry.RegisterListener(sp.GetRequiredService<StocksSuccessListener>());
        registry.RegisterListener(sp.GetRequiredService<StocksSubscriptionListener>());
        registry.RegisterListener(sp.GetRequiredService<StocksTradeListener>());

        var client = new WebSocketClient(
            sp.GetRequiredService<ILogger<WebSocketClient>>(),
            wsOptions.Host,
            wsOptions.BufferSize,
            serializer,
            [],
            registry
        );

        return new WebSocketWorker(
            sp.GetRequiredService<ILogger<WebSocketWorker>>(),
            client,
            Options.Create(wsOptions)
        );
    }
}
