using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets;
using Ouranos.Pantheon.Modules.Plutus.Features.Markets.GetAllMarkets.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using Ouranos.Pantheon.Tests.Utils.Extensions;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Markets.GetAllMarkets;

public sealed class GetAllMarketsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllMarketsHandler _handler;
    private readonly ILogger<GetAllMarketsHandler> _logger = Substitute.For<ILogger<GetAllMarketsHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetAllMarketsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllMarketsHandler(_logger, _dbContext);
    }

    [Fact]
    public async Task Handle_WhenMarketsExist_ShouldReturnAllMarkets()
    {
        // Arrange
        var markets = _fixture.CreateMany<Market>(3).ToArray();
        await _dbContext.SeedData(markets);

        var query = new GetAllMarketsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.Count.ShouldBe(3);
    }

    [Fact]
    public async Task Handle_WhenNoMarkets_ShouldReturnEmptyList()
    {
        // Arrange
        var query = new GetAllMarketsInput();

        // Act
        var result = await _handler.Handle(query, CancellationToken.None);

        // Assert
        result.ShouldNotBeNull();
        result.ShouldBeEmpty();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetAllMarketsInput();
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
