using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Forecasts.InsertForecasts;

public sealed class InsertForecastsHandler : CommandHandler<InsertForecastsInput>
{
    private readonly IRepository<Forecast> _forecastRepository;
    private readonly ILogger<InsertForecastsHandler> _logger;

    public InsertForecastsHandler(
        ILogger<InsertForecastsHandler> logger,
        IRepository<Forecast> forecastRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(forecastRepository);

        _logger = logger;
        _forecastRepository = forecastRepository;
    }

    public override async Task Handle(
        InsertForecastsInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle insert forecasts command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        await _forecastRepository.Delete(_ => true, cancellationToken);
        await _forecastRepository.CreateMany(command.Forecasts, cancellationToken);

        _logger.LogDebug("Successfully handled insert forecasts command.");
    }
}