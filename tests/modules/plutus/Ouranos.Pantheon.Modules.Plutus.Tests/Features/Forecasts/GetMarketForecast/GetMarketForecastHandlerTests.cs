using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetMarketForecast.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.GetMarketForecast;

public sealed class GetMarketForecastHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetMarketForecastHandler _handler;
    private readonly ILogger<GetMarketForecastHandler> _logger = Substitute.For<ILogger<GetMarketForecastHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetMarketForecastHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetMarketForecastHandler(_logger, _dbContext, Options.Create(new QueryOptions()));
    }

    [Fact]
    public async Task Handle_WhenMarketNotFound_ShouldThrowNotFoundException()
    {
        // Arrange
        var query = new GetMarketForecastInput(
            new Id<Market>(_fixture.Create<string>()),
            Take: 10
        );

        // Act
        var get = async () => await _handler.Handle(query, CancellationToken.None);

        // Assert
        await get.ShouldThrowAsync<Exception>();
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var query = new GetMarketForecastInput(
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
