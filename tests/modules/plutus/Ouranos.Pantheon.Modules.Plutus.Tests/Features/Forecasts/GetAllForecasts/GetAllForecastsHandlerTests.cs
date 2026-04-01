using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Tests.Utils.AutoFixture.IdConfiguration;
using DbContextExtensions = Ouranos.Pantheon.Tests.Utils.Extensions.DbContextExtensions;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.GetAllForecasts;

public sealed class GetAllForecastsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly GetAllForecastsHandler _handler;
    private readonly ILogger<GetAllForecastsHandler> _logger = Substitute.For<ILogger<GetAllForecastsHandler>>();
    private readonly PlutusDbContext _dbContext;

    public GetAllForecastsHandlerTests()
    {
        _fixture.Customize(new IdCustomization());

        _dbContext = DbContextExtensions.Mock<PlutusDbContext>();
        _handler = new GetAllForecastsHandler(_logger, _dbContext, Options.Create(new QueryOptions()));
    }

    [Fact]
    public async Task Handle_WhenNoForecasts_ShouldReturnEmptyPagedResponse()
    {
        // Arrange
        var query = new GetAllForecastsInput(Take: 10);

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
        var query = new GetAllForecastsInput(Take: 10);
        var cancellationToken = new CancellationToken(true);

        // Act
        var get = async () => await _handler.Handle(query, cancellationToken);

        // Assert
        await get.ShouldThrowAsync<OperationCanceledException>();
    }
}
