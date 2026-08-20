using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.ForecastEvaluator;
using Ouranos.Pantheon.Modules.Plutus.Shared;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using TickerQ.Utilities.Base;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.ForecastEvaluator;

public sealed class ForecastEvaluatorJobTests
{
    private readonly ILogger<ForecastEvaluatorJob> _logger = Substitute.For<
        ILogger<ForecastEvaluatorJob>
    >();
    private readonly PlutusDbContext _dbContext;
    private readonly TickerFunctionContext _tickerFunctionContext;
    private readonly ForecastEvaluatorJob _job;

    public ForecastEvaluatorJobTests()
    {
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _tickerFunctionContext = new TickerFunctionContext();

        _job = new ForecastEvaluatorJob(
            _logger,
            _dbContext,
            Options.Create(
                new PlutusOptions
                {
                    Forecasting = new ForecastingOptions(
                        IsEnabled: true,
                        NumPredictions: 7,
                        HistoryDays: 30,
                        BatchSize: 500,
                        ModelName: "plutus-forecasting-v1",
                        MaxEvaluationAgeDays: 90
                    ),
                }
            )
        );
    }

    [Fact]
    public async Task Execute_WhenForecastingIsDisabled_ShouldSkipEvaluation()
    {
        // Arrange
        var job = new ForecastEvaluatorJob(
            _logger,
            _dbContext,
            Options.Create(
                new PlutusOptions
                {
                    Forecasting = new ForecastingOptions() with { IsEnabled = false },
                }
            )
        );
        var record = CreatePendingRecord(DateTimeOffset.UtcNow.AddDays(-1));
        await _dbContext.SeedData(record);

        // Act
        await job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        _dbContext.ForecastRecords.Single().EvaluatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Execute_WhenNoPendingRecords_ShouldNotSaveChanges()
    {
        // Arrange
        var record = CreatePendingRecord(DateTimeOffset.UtcNow.AddDays(-1));
        record.RecordActual(new ForecastPoint(100m, 90m, 110m, 50m), DateTimeOffset.UtcNow);
        await _dbContext.SeedData(record);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        _dbContext.ForecastRecords.Single().EvaluatedAt.ShouldNotBeNull();
    }

    [Fact]
    public async Task Execute_WhenPendingRecordHasMatchingTrades_ShouldRecordActual()
    {
        // Arrange
        var targetDay = new DateTimeOffset(
            DateTimeOffset.UtcNow.UtcDateTime.Date,
            TimeSpan.Zero
        ).AddDays(-1);
        var market = CreateMarket();
        var symbol = CreateSymbol(market.Id);
        var record = CreatePendingRecord(targetDay, symbol.Id);
        var trade = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            120m,
            10m,
            targetDay.AddHours(12)
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(record);
        await _dbContext.SeedData(trade);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        var evaluated = _dbContext.ForecastRecords.Single();
        evaluated.EvaluatedAt.ShouldNotBeNull();
        evaluated.Actual.ShouldNotBeNull();
        evaluated.Actual!.AveragePrice.ShouldBe(120m);
    }

    [Fact]
    public async Task Execute_WhenPendingRecordHasNoMatchingTrades_ShouldNotRecordActual()
    {
        // Arrange
        var targetDay = new DateTimeOffset(
            DateTimeOffset.UtcNow.UtcDateTime.Date,
            TimeSpan.Zero
        ).AddDays(-1);
        var market = CreateMarket();
        var symbol = CreateSymbol(market.Id);
        var record = CreatePendingRecord(targetDay, symbol.Id);

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(record);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        _dbContext.ForecastRecords.Single().EvaluatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Execute_WhenPendingRecordIsOlderThanMaxAge_ShouldNotEvaluate()
    {
        // Arrange
        var tooOldTargetDay = new DateTimeOffset(
            DateTimeOffset.UtcNow.UtcDateTime.Date,
            TimeSpan.Zero
        ).AddDays(-(90 + 1));
        var market = CreateMarket();
        var symbol = CreateSymbol(market.Id);
        var record = CreatePendingRecord(tooOldTargetDay, symbol.Id);
        var trade = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbol.Id,
            120m,
            10m,
            tooOldTargetDay.AddHours(12)
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(record);
        await _dbContext.SeedData(trade);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        _dbContext.ForecastRecords.Single().EvaluatedAt.ShouldBeNull();
    }

    [Fact]
    public async Task Execute_WithMultipleSymbols_ShouldEvaluateEachIndependently()
    {
        // Arrange
        var targetDay = new DateTimeOffset(
            DateTimeOffset.UtcNow.UtcDateTime.Date,
            TimeSpan.Zero
        ).AddDays(-1);
        var market = CreateMarket();
        var symbolA = CreateSymbol(market.Id);
        var symbolB = CreateSymbol(market.Id);
        var recordA = CreatePendingRecord(targetDay, symbolA.Id);
        var recordB = CreatePendingRecord(targetDay, symbolB.Id);
        var tradeA = Trade.Create(
            new Id<Trade>(Guid.NewGuid().ToString()),
            symbolA.Id,
            100m,
            5m,
            targetDay.AddHours(6)
        );
        // No trade for symbolB

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbolA, symbolB);
        await _dbContext.SeedData(recordA, recordB);
        await _dbContext.SeedData(tradeA);

        // Act
        await _job.Execute(_tickerFunctionContext, CancellationToken.None);

        // Assert
        _dbContext
            .ForecastRecords.Single(r => r.SymbolId == symbolA.Id)
            .EvaluatedAt.ShouldNotBeNull();
        _dbContext.ForecastRecords.Single(r => r.SymbolId == symbolB.Id).EvaluatedAt.ShouldBeNull();
    }

    private static Market CreateMarket()
    {
        return Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test Market",
            new Taxes(null)
        );
    }

    private static Symbol CreateSymbol(Id<Market> marketId)
    {
        return Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            Guid.NewGuid().ToString(),
            null,
            "Test Symbol",
            marketId,
            new AdditionalFields()
        );
    }

    private static ForecastRecord CreatePendingRecord(
        DateTimeOffset targetAt,
        Id<Symbol>? symbolId = null
    )
    {
        return ForecastRecord.Create(
            new Id<ForecastRecord>(Guid.NewGuid().ToString()),
            new Id<ForecastRun>(Guid.NewGuid().ToString()),
            new Id<Market>(Guid.NewGuid().ToString()),
            symbolId ?? new Id<Symbol>(Guid.NewGuid().ToString()),
            "plutus-forecasting-v1",
            DateTimeOffset.UtcNow.AddDays(-2),
            targetAt,
            1,
            new ForecastPoint(100m, 90m, 110m, 50m)
        );
    }
}
