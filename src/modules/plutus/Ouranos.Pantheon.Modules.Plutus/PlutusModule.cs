using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskMarketAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskMarketAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.AskSymbolAnalyst.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.FindSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetActivityHours;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecast;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetForecastLeaders;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPositions;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetPriceHistory;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignalRankings;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSignals;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Assistants.Tools.SetExplorerView;
using Ouranos.Pantheon.Modules.Plutus.Features.DataLoaders;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastGenerator;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.CreateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.DeleteMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.UpdateMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.ClosePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.CreatePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetAllPositions;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.GetPosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.LinkPosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Positions.UpdatePosition;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.DeleteRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetAllRecipes;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignalHistory;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CancelBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.CreateStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.DeleteStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllBacktests;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetAllStrategies;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetRecommendations;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.OptimizeStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RestartBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.RunBacktest;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.SetStrategyActive;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.Shared;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.UpdateStrategy;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.UpdateSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetAllTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketOverview;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetMarketTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetRecipeTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetSymbolTrades;
using Ouranos.Pantheon.Modules.Plutus.Features.Trades.GetVolumeHeatmap;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals.Computers;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Engine;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Events;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Flip;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Optimization;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Strategies.Swing;
using Ouranos.Pantheon.Modules.Shared.Contract;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Ouranos.Pantheon.Modules.Plutus;

public sealed class PlutusModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        var plutusOptionsSection = builder.Configuration.GetSection(PlutusOptions.SectionName);

        builder
            .Services.AddCorePostgresModule<PlutusDbContext>(
                builder.Configuration,
                typeof(PlutusModule).Assembly
            )
            .Configure<PlutusOptions>(plutusOptionsSection)
            .Configure<OptimizationOptions>(
                plutusOptionsSection.GetSection(OptimizationOptions.SectionName)
            )
            .Configure<BacktestDataOptions>(
                plutusOptionsSection.GetSection(BacktestDataOptions.SectionName)
            )
            .Configure<FlipDataOptions>(
                plutusOptionsSection.GetSection(FlipDataOptions.SectionName)
            );

        builder.Services.AddHttpClient<IForecastModelClient, ForecastModelClient>(
            OuranosMachineLearningModule.ConfigureHttpClient
        );

        builder.AddDataLoaders();
        ConfigureSignalComputers(builder);
        ConfigureStrategyEngines(builder);
        ConfigureAssistantTools(builder);
        return builder;
    }

    private static void ConfigureAssistantTools(IHostApplicationBuilder builder)
    {
        builder
            .Services.AddAssistantTool<SymbolAnalystContext, GetPriceHistoryTool>()
            .AddAssistantTool<SymbolAnalystContext, GetSignalsTool>()
            .AddAssistantTool<SymbolAnalystContext, GetForecastTool>()
            .AddAssistantTool<SymbolAnalystContext, GetPositionsTool>()
            .AddAssistantTool<SymbolAnalystContext, FindSymbolsTool>()
            .AddAssistantTool<SymbolAnalystContext, GetSymbolTool>();

        builder
            .Services.AddAssistantTool<MarketAnalystContext, FindSymbolsTool>()
            .AddAssistantTool<MarketAnalystContext, GetSymbolTool>()
            .AddAssistantTool<MarketAnalystContext, GetSignalRankingsTool>()
            .AddAssistantTool<MarketAnalystContext, GetForecastLeadersTool>()
            .AddAssistantTool<MarketAnalystContext, GetActivityHoursTool>()
            .AddAssistantTool<MarketAnalystContext, SetExplorerViewTool>();
    }

    public async Task<IHost> Configure(IHost host)
    {
        await host.Services.ApplyCorePostgresMigrations<PlutusDbContext>();
        await host
            .Services.GetRequiredService<BacktestRecovery>()
            .RecoverInterruptedBacktestsAsync();
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
        GetMarketOverviewEndpoint.Map(app);
        GetAllTradesEndpoint.Map(app);
        GetVolumeHeatmapEndpoint.Map(app);
        GetRecipeTradesEndpoint.Map(app);
        GetSymbolTradesEndpoint.Map(app);

        GetAllSymbolsEndpoint.Map(app);
        GetSymbolEndpoint.Map(app);
        GetDailySymbolSummaryEndpoint.Map(app);
        AskSymbolAnalystEndpoint.Map(app);
        AskMarketAnalystEndpoint.Map(app);

        GetAllForecastsEndpoint.Map(app);
        GetMarketForecastEndpoint.Map(app);
        GetForecastEfficacyEndpoint.Map(app);

        GetAllRecipesEndpoint.Map(app);
        GetRecipeEndpoint.Map(app);
        CreateRecipeEndpoint.Map(app);
        UpdateRecipeEndpoint.Map(app);
        DeleteRecipeEndpoint.Map(app);

        GetSymbolSignalsEndpoint.Map(app);
        GetSymbolSignalHistoryEndpoint.Map(app);
        GetSignalRankingsEndpoint.Map(app);

        CreateSymbolGroupEndpoint.Map(app);
        GetAllSymbolGroupsEndpoint.Map(app);
        GetSymbolGroupEndpoint.Map(app);
        UpdateSymbolGroupEndpoint.Map(app);
        DeleteSymbolGroupEndpoint.Map(app);

        CreateStrategyEndpoint.Map(app);
        GetAllStrategiesEndpoint.Map(app);
        GetStrategyEndpoint.Map(app);
        UpdateStrategyEndpoint.Map(app);
        DeleteStrategyEndpoint.Map(app);
        SetStrategyActiveEndpoint.Map(app);

        GetAllBacktestsEndpoint.Map(app);
        GetActiveBacktestsEndpoint.Map(app);
        GetBacktestEndpoint.Map(app);
        CancelBacktestEndpoint.Map(app);
        RestartBacktestEndpoint.Map(app);
        GetRecommendationsEndpoint.Map(app);
        RunBacktestEndpoint.Map(app);
        OptimizeStrategyEndpoint.Map(app);

        CreatePositionEndpoint.Map(app);
        GetAllPositionsEndpoint.Map(app);
        GetPositionEndpoint.Map(app);
        UpdatePositionEndpoint.Map(app);
        ClosePositionEndpoint.Map(app);
        LinkPositionEndpoint.Map(app);
    }

    public void ConfigureWolverine(WolverineOptions opts, IConfiguration configuration)
    {
        opts.CodeGeneration.AlwaysUseServiceLocationFor<PlutusDbContext>();

        opts.PublishMessage<RunBacktestMessage>()
            .ToRabbitExchange(
                RunBacktestMessage.Exchange,
                e =>
                {
                    e.BindQueue(RunBacktestMessage.Queue);
                }
            );

        opts.ListenToRabbitQueue(RunBacktestMessage.Queue)
            .DeadLetterQueueing(new DeadLetterQueue(RunBacktestMessage.DeadLetterQueue));

        opts.PublishMessage<OptimizeStrategyMessage>()
            .ToRabbitExchange(
                OptimizeStrategyMessage.Exchange,
                e =>
                {
                    e.BindQueue(OptimizeStrategyMessage.Queue);
                }
            );

        opts.ListenToRabbitQueue(OptimizeStrategyMessage.Queue)
            .DeadLetterQueueing(new DeadLetterQueue(OptimizeStrategyMessage.DeadLetterQueue));

        opts.ConfigureDataLoaderMessaging(configuration);
    }

    private static void ConfigureSignalComputers(IHostApplicationBuilder builder)
    {
        var plutusOptionsSection = builder.Configuration.GetSection(PlutusOptions.SectionName);

        builder
            .Services.Configure<SignalOptions>(
                plutusOptionsSection.GetSection(SignalOptions.SectionName)
            )
            .AddSingleton<ISignalComputer, TaxAdjustedRoiSignalComputer>()
            .AddSingleton<ISignalComputer, VolumeAnomalySignalComputer>()
            .AddSingleton<ISignalComputer, TrendMomentumSignalComputer>()
            .AddSingleton<ISignalComputer, BollingerBandsSignalComputer>()
            .AddSingleton<ISignalComputer, RsiSignalComputer>()
            .AddSingleton<ISignalComputer, MovingAverageCrossoverSignalComputer>()
            .AddSingleton<ISignalComputer, PriceVelocitySignalComputer>();
    }

    private static void ConfigureStrategyEngines(IHostApplicationBuilder builder)
    {
        builder
            .Services.AddSingleton<ISwingDataQueryService, SwingDataQueryService>()
            .AddSingleton<IFlipDataQueryService, FlipDataQueryService>()
            .AddSingleton<SwingSignalReconstructor>()
            .AddSingleton<IStrategyEngine, SwingEngine>()
            .AddSingleton<IStrategyEngine, FlipEngine>()
            .AddSingleton<StrategyEngines>()
            .AddSingleton<StrategyOptimizer>()
            .AddSingleton<BacktestJobRunner>()
            .AddSingleton<BacktestRecovery>();
    }
}
