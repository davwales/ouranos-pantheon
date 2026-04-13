using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.GetForecastEfficacy;

public sealed class GetForecastEfficacyHandlerTests
{
    private readonly ILogger<GetForecastEfficacyHandler> _logger =
        Substitute.For<ILogger<GetForecastEfficacyHandler>>();

    private readonly PlutusDbContext _dbContext;
    private readonly GetForecastEfficacyHandler _handler;

    public GetForecastEfficacyHandlerTests()
    {
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetForecastEfficacyHandler(_logger, _dbContext, Options.Create(new QueryOptions()));
    }

    [Fact]
    public async Task Handle_WhenNoEvaluatedRecords_ShouldReturnEmptyResult()
    {
        // Act
        var result = await _handler.Handle(new GetForecastEfficacyInput(), CancellationToken.None);

        // Assert
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenEvaluatedRecordsExist_ShouldReturnAggregatedEfficacy()
    {
        // Arrange
        var (market, symbol, run) = await SeedBaseEntitiesAsync();
        var record = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 110m, actual: 100m);
        await _dbContext.SeedData(record);

        // Act
        var result = await _handler.Handle(new GetForecastEfficacyInput(), CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        var item = result.Items.Single();
        item.SymbolId.ShouldBe(symbol.Id);
        item.SymbolName.ShouldBe(symbol.Name);
        item.MarketId.ShouldBe(market.Id);
        item.ModelName.ShouldBe("plutus-forecasting-v1");
        item.HorizonDays.ShouldBe(1);
        item.EvaluatedCount.ShouldBe(1);
        item.MeanAbsoluteError.ShouldBe(10m);
        item.MeanAbsolutePercentageError.ShouldBe(0.1m);
        item.MeanBias.ShouldBe(10m);
    }

    [Fact]
    public async Task Handle_WhenMultipleRecordsForSameSymbol_ShouldAverageMetrics()
    {
        // Arrange
        var (market, symbol, run) = await SeedBaseEntitiesAsync();
        var record1 = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 110m, actual: 100m, horizonDays: 1);
        var record2 = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 90m, actual: 100m, horizonDays: 1);
        await _dbContext.SeedData(record1, record2);

        // Act
        var result = await _handler.Handle(new GetForecastEfficacyInput(), CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        var item = result.Items.Single();
        item.EvaluatedCount.ShouldBe(2);
        item.MeanAbsoluteError.ShouldBe(10m);
        item.MeanBias.ShouldBe(0m);
    }

    [Fact]
    public async Task Handle_WhenUnevaluatedRecordsExist_ShouldExcludeThem()
    {
        // Arrange
        var (market, symbol, run) = await SeedBaseEntitiesAsync();
        var unevaluated = ForecastRecord.Create(
            new Id<ForecastRecord>(Guid.NewGuid().ToString()),
            run.Id, market.Id, symbol.Id,
            "plutus-forecasting-v1",
            DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1),
            1,
            new ForecastPoint(110m, 100m, 120m, 50m)
        );
        await _dbContext.SeedData(unevaluated);

        // Act
        var result = await _handler.Handle(new GetForecastEfficacyInput(), CancellationToken.None);

        // Assert
        result.Items.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenFilteredBySymbolId_ShouldReturnOnlyMatchingSymbol()
    {
        // Arrange
        var (market, symbolA, run) = await SeedBaseEntitiesAsync();
        var symbolB = await SeedSymbolAsync(market.Id);
        var recordA = CreateEvaluatedRecord(run.Id, market.Id, symbolA.Id, predicted: 110m, actual: 100m);
        var recordB = CreateEvaluatedRecord(run.Id, market.Id, symbolB.Id, predicted: 110m, actual: 100m);
        await _dbContext.SeedData(recordA, recordB);

        var input = new GetForecastEfficacyInput(SymbolId: symbolA.Id.Value);

        // Act
        var result = await _handler.Handle(input, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.Single().SymbolId.ShouldBe(symbolA.Id);
    }

    [Fact]
    public async Task Handle_WhenFilteredByModelName_ShouldReturnOnlyMatchingModel()
    {
        // Arrange
        var (market, symbol, run) = await SeedBaseEntitiesAsync();
        var recordV1 = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 110m, actual: 100m, modelName: "plutus-forecasting-v1");
        var recordV2 = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 105m, actual: 100m, modelName: "plutus-forecasting-v2");
        await _dbContext.SeedData(recordV1, recordV2);

        var input = new GetForecastEfficacyInput(ModelName: "plutus-forecasting-v2");

        // Act
        var result = await _handler.Handle(input, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.Single().ModelName.ShouldBe("plutus-forecasting-v2");
    }

    [Fact]
    public async Task Handle_WhenFilteredByHorizonDays_ShouldReturnOnlyMatchingHorizon()
    {
        // Arrange
        var (market, symbol, run) = await SeedBaseEntitiesAsync();
        var record1Day = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 110m, actual: 100m, horizonDays: 1);
        var record3Day = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 115m, actual: 100m, horizonDays: 3);
        await _dbContext.SeedData(record1Day, record3Day);

        var input = new GetForecastEfficacyInput(HorizonDays: 3);

        // Act
        var result = await _handler.Handle(input, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
        result.Items.Single().HorizonDays.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_WhenFilteredBySince_ShouldExcludeOlderRecords()
    {
        // Arrange
        var (market, symbol, run) = await SeedBaseEntitiesAsync();
        var recentRecord = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 110m, actual: 100m,
            generatedAt: DateTimeOffset.UtcNow.AddDays(-1));
        var oldRecord = CreateEvaluatedRecord(run.Id, market.Id, symbol.Id, predicted: 110m, actual: 100m,
            generatedAt: DateTimeOffset.UtcNow.AddDays(-10));
        await _dbContext.SeedData(recentRecord, oldRecord);

        var input = new GetForecastEfficacyInput(Since: DateTimeOffset.UtcNow.AddDays(-5));

        // Act
        var result = await _handler.Handle(input, CancellationToken.None);

        // Assert
        result.TotalCount.ShouldBe(1);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Act
        var act = async () => await _handler.Handle(
            new GetForecastEfficacyInput(),
            new CancellationToken(true)
        );

        // Assert
        await act.ShouldThrowAsync<OperationCanceledException>();
    }

    private async Task<(Market market, Symbol symbol, ForecastRun run)> SeedBaseEntitiesAsync()
    {
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            "Test Market",
            new Taxes(null)
        );
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            Guid.NewGuid().ToString(),
            null,
            "Test Symbol",
            market.Id,
            new AdditionalFields()
        );
        var run = ForecastRun.Create(
            new Id<ForecastRun>(Guid.NewGuid().ToString()),
            "plutus-forecasting-v1",
            DateTimeOffset.UtcNow.AddDays(-2)
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(run);

        return (market, symbol, run);
    }

    private async Task<Symbol> SeedSymbolAsync(Id<Market> marketId)
    {
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            Guid.NewGuid().ToString(),
            null,
            "Other Symbol",
            marketId,
            new AdditionalFields()
        );
        await _dbContext.SeedData(symbol);
        return symbol;
    }

    private static ForecastRecord CreateEvaluatedRecord(
        Id<ForecastRun> runId,
        Id<Market> marketId,
        Id<Symbol> symbolId,
        decimal predicted,
        decimal actual,
        int horizonDays = 1,
        string modelName = "plutus-forecasting-v1",
        DateTimeOffset? generatedAt = null
    )
    {
        var record = ForecastRecord.Create(
            new Id<ForecastRecord>(Guid.NewGuid().ToString()),
            runId,
            marketId,
            symbolId,
            modelName,
            generatedAt ?? DateTimeOffset.UtcNow.AddDays(-2),
            DateTimeOffset.UtcNow.AddDays(-1),
            horizonDays,
            new ForecastPoint(predicted, predicted * 0.9m, predicted * 1.1m, 50m)
        );
        record.RecordActual(new ForecastPoint(actual, actual * 0.9m, actual * 1.1m, 50m), DateTimeOffset.UtcNow);
        return record;
    }
}
