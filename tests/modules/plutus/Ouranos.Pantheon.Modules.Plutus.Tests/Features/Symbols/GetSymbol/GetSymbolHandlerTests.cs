using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetSymbol.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.GetSymbol;

public sealed class GetSymbolHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetSymbolHandler _handler;
    private readonly ILogger<GetSymbolHandler> _logger = Substitute.For<ILogger<GetSymbolHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetSymbolHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetSymbolHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnSymbolResponse()
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

        await _dbContext.SeedData(market);
        await _dbContext.SeedData(symbol);

        var query = new GetSymbolInput(symbol.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Id.ShouldBe(symbol.Id);
        result.Code.ShouldBe(symbol.Code);
        result.Name.ShouldBe(symbol.Name);
        result.MarketId.ShouldBe(symbol.MarketId);
        result.AdditionalFields.ShouldNotBeNull();
    }

    [Fact]
    public async Task Handle_WhenSymbolNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetSymbolInput(new Id<Symbol>(_fixture.Create<string>()));

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetSymbolInput(new Id<Symbol>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
