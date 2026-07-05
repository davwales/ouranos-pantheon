using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.GetSignalRankings;

public sealed class GetSignalRankingsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly ILogger<GetSignalRankingsHandler> _logger = Substitute.For<
        ILogger<GetSignalRankingsHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetSignalRankingsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());
        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
    }

    private GetSignalRankingsHandler BuildHandler(IEnumerable<ISignalComputer>? computers = null)
    {
        return new(_logger, _dbContext, Options.Create(new QueryOptions()), computers ?? []);
    }

    private async Task SeedLatestSignals(
        params (Id<Symbol> SymbolId, SignalType Type, decimal Value)[] rows
    )
    {
        await _dbContext.LatestSignals.AddRangeAsync(
            rows.Select(r => new LatestSignal(r.SymbolId, r.Type, r.Value))
        );
        await _dbContext.SaveChangesAsync();
    }

    [Fact]
    public async Task Handle_WhenNoSignals_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var handler = BuildHandler();
        var query = new GetSignalRankingsInput(new Id<Market>(_fixture.Create<string>()), Take: 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenSignalsExist_ShouldReturnRankings()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            new Taxes(null)
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await SeedLatestSignals((symbol.Id, SignalType.BollingerBands, 0.5m));

        var handler = BuildHandler();
        var query = new GetSignalRankingsInput(market.Id, Take: 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        result.TotalCount.ShouldBe(1);
        var item = result.Items.First();
        item.OverallScore.ShouldBe(0.5m);
        item.SymbolId.ShouldBe(symbol.Id);
        item.SymbolName.ShouldBe(symbol.Name);
        item.SignalCount.ShouldBe(1);
        item.BullishCount.ShouldBe(1);
        item.BearishCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var handler = BuildHandler();
        var query = new GetSignalRankingsInput(new Id<Market>(_fixture.Create<string>()), Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task Handle_WhenComputersHaveIntents_ShouldBuildIntentMap()
    {
        // Arrange
        var computer = Substitute.For<ISignalComputer>();
        computer.Intents.Returns([InvestmentIntent.Buy, InvestmentIntent.Sell]);
        computer.Type.Returns(SignalType.Rsi);

        var handler = BuildHandler([computer]);
        var query = new GetSignalRankingsInput(new Id<Market>(_fixture.Create<string>()), Take: 10);

        // Act
        var result = await handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
    }
}
