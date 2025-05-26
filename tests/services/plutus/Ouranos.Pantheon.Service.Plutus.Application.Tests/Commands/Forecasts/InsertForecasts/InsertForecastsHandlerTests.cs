using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Service.Plutus.Application.Commands.Forecasts.InsertForecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.Application.Tests.Commands.Forecasts.InsertForecasts;

public sealed class InsertForecastsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly IRepository<Forecast> _forecastRepository = Substitute.For<IRepository<Forecast>>();
    private readonly InsertForecastsHandler _handler;
    private readonly ILogger<InsertForecastsHandler> _logger = Substitute.For<ILogger<InsertForecastsHandler>>();

    public InsertForecastsHandlerTests()
    {
        _handler = new InsertForecastsHandler(_logger, _forecastRepository);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteExistingAndCreateNewForecasts()
    {
        // Arrange
        var command = _fixture.Create<InsertForecastsInput>();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _forecastRepository.Received(1).Delete(
            Arg.Any<Expression<Func<Forecast, bool>>>(),
            Arg.Any<CancellationToken>()
        );

        await _forecastRepository.Received(1).CreateMany(
            Arg.Is(command.Forecasts),
            Arg.Any<CancellationToken>()
        );
    }

    [Fact]
    public async Task Handle_WhenCancelled_ShouldThrowOperationCanceledException()
    {
        // Arrange
        var command = _fixture.Create<InsertForecastsInput>();
        var cancellationToken = new CancellationToken(true);

        // Act
        var insert = async () => await _handler.Handle(command, cancellationToken);

        // Assert
        await insert.ShouldThrowAsync<OperationCanceledException>();
    }
}