using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.GetAllSymbolGroups;

public sealed class GetAllSymbolGroupsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllSymbolGroupsHandler _handler;
    private readonly ILogger<GetAllSymbolGroupsHandler> _logger = Substitute.For<
        ILogger<GetAllSymbolGroupsHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetAllSymbolGroupsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllSymbolGroupsHandler(
            _logger,
            _dbContext,
            Options.Create(new QueryOptions())
        );
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
    public async Task Handle_WhenGroupsExist_ShouldReturnPagedGroups()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var groups = Enumerable
            .Range(0, 2)
            .Select(_ =>
                SymbolGroup.Create(
                    new Id<SymbolGroup>(Guid.NewGuid().ToString()),
                    _fixture.Create<string>(),
                    null,
                    market.Id
                )
            )
            .ToArray();

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(groups);

        var query = new GetAllSymbolGroupsInput(market.Id, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(2);
        result.TotalCount.ShouldBe(2);
        var item = result.Items.First();
        item.Id.ShouldNotBe(default);
        item.Name.ShouldNotBeNull();
        item.MarketId.ShouldBe(market.Id);
    }

    [Fact]
    public async Task Handle_WhenGroupHasMembersWithSnapshots_ShouldProjectGroupStats()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var member = SymbolGroupMember.Create(
            new Id<SymbolGroup>(Guid.NewGuid().ToString()),
            symbol.Id
        );

        var group = SymbolGroup.Create(
            member.SymbolGroupId,
            _fixture.Create<string>(),
            null,
            market.Id,
            members: [member]
        );

        var snapshot = new MarketTradeSnapshot(
            market.Id,
            symbol.Id,
            TimeFrame.OneDay,
            TotalSpent: 10000m,
            MinPrice: 90m,
            MaxPrice: 110m,
            TotalVolume: 100m,
            NumTransactions: 50,
            Limit: 200m,
            Tax: 1m
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(group);
        await _dbContext.SeedData(snapshot);

        var query = new GetAllSymbolGroupsInput(market.Id, TimeFrame: TimeFrame.OneDay, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        var item = result.Items.First();
        item.SymbolCount.ShouldBe(1);
        item.TotalVolume.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenNoGroups_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetAllSymbolGroupsInput(
            new Id<Market>(_fixture.Create<string>()),
            Take: 10
        );

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenGroupHasMembersWithSignals_ShouldProjectSignalScores()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );
        var symbol = Symbol.Create(
            new Id<Symbol>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            null,
            _fixture.Create<string>(),
            market.Id,
            new AdditionalFields()
        );

        var member = SymbolGroupMember.Create(
            new Id<SymbolGroup>(Guid.NewGuid().ToString()),
            symbol.Id
        );
        var group = SymbolGroup.Create(
            member.SymbolGroupId,
            _fixture.Create<string>(),
            null,
            market.Id,
            members: [member]
        );

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);
        await _dbContext.SeedData(group);
        await SeedLatestSignals(
            (symbol.Id, SignalType.BollingerBands, 0.8m),
            (symbol.Id, SignalType.Rsi, -0.4m)
        );

        var query = new GetAllSymbolGroupsInput(market.Id, Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(1);
        var item = result.Items.First();
        item.AverageOverallScore.ShouldNotBeNull();
        item.AverageOverallScore.ShouldBe((0.8m + -0.4m) / 2m);
        item.BullishCount.ShouldBe(1);
        item.BearishCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllSymbolGroupsInput(
            new Id<Market>(_fixture.Create<string>()),
            Take: 10
        );
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
