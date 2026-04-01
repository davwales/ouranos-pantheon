using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetMarket.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.GetMarket;

public sealed class GetMarketHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetMarketHandler _handler;
    private readonly ILogger<GetMarketHandler> _logger = Substitute.For<ILogger<GetMarketHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetMarketHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetMarketHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldReturnMarketResponse()
    {
        // Arrange
        var existingMarket = _fixture.Create<Market>();
        await _dbContext.SeedData(existingMarket);

        var query = new GetMarketInput(existingMarket.Id);

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldBeOfType<GetMarketResponse>();
        result.Id.ShouldBe(existingMarket.Id);
        result.Name.ShouldBe(existingMarket.Name);
        result.Taxes.ShouldBe(existingMarket.Taxes);
        result.IsForecastingEnabled.ShouldBe(existingMarket.IsForecastingEnabled);
    }

    [Fact]
    public async Task Handle_WhenMarketNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetMarketInput(new Id<Market>(_fixture.Create<string>()));

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetMarketInput(new Id<Market>(_fixture.Create<string>()));
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
