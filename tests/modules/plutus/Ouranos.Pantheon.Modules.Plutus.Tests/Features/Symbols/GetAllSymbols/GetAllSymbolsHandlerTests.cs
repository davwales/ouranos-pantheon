using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.GetAllSymbols;

public sealed class GetAllSymbolsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllSymbolsHandler _handler;
    private readonly ILogger<GetAllSymbolsHandler> _logger = Substitute.For<
        ILogger<GetAllSymbolsHandler>
    >();
    private readonly PlutusDbContext _dbContext;

    public GetAllSymbolsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllSymbolsHandler(
            _logger,
            _dbContext,
            Options.Create(new QueryOptions())
        );
    }

    [Fact]
    public async Task Handle_WhenSymbolsExist_ShouldReturnPagedSymbols()
    {
        // Arrange
        var market = Market.Create(
            new Id<Market>(Guid.NewGuid().ToString()),
            _fixture.Create<string>(),
            _fixture.Create<Taxes>()
        );

        var symbols = Enumerable
            .Range(0, 3)
            .Select(_ =>
                Symbol.Create(
                    new Id<Symbol>(Guid.NewGuid().ToString()),
                    _fixture.Create<string>(),
                    null,
                    _fixture.Create<string>(),
                    market.Id,
                    new AdditionalFields()
                )
            )
            .ToArray();

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbols);

        var query = new GetAllSymbolsInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.Count().ShouldBe(3);
        result.TotalCount.ShouldBe(3);
        var item = result.Items.First();
        item.Id.ShouldNotBe(default);
        item.Code.ShouldNotBeNull();
        item.Name.ShouldNotBeNull();
        item.MarketId.ShouldNotBe(default);
    }

    [Fact]
    public async Task Handle_WhenNoSymbols_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetAllSymbolsInput(Take: 10);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Items.ShouldBeEmpty();
        result.TotalCount.ShouldBe(0);
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllSymbolsInput(Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
