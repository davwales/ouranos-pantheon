using System.Linq.Expressions;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Plutus.Service.Application.Commands.Forecasts.InsertForecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

namespace Ouranos.Pantheon.Plutus.Service.Application.Tests.Commands.Forecasts.InsertForecasts;

public sealed class InsertForecastsHandlerTests
{
    private readonly IFixture _fixture = new Fixture();
    private readonly InsertForecastsHandler _handler;
    private readonly ILogger<InsertForecastsHandler> _logger = Substitute.For<ILogger<InsertForecastsHandler>>();
    private readonly IPlutusUnitOfWork _unitOfWork = Substitute.For<IPlutusUnitOfWork>();

    public InsertForecastsHandlerTests()
    {
        _handler = new InsertForecastsHandler(_logger, _unitOfWork);
    }

    [Fact]
    public async Task Handle_WhenHappyPath_ShouldDeleteExistingAndCreateNewForecasts()
    {
        // Arrange
        var command = _fixture.Create<InsertForecastsInput>();

        // Act
        await _handler.Handle(command, CancellationToken.None);

        // Assert
        await _unitOfWork.Forecasts.Received(1).Delete(
            Arg.Any<Expression<Func<Forecast, bool>>>(),
            Arg.Any<CancellationToken>()
        );

        await _unitOfWork.Forecasts.Received(1).CreateMany(
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