using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Forecasts.InsertForecasts;

public sealed class InsertForecastsHandler : CommandHandler<InsertForecastsInput>
{
    private readonly IPlutusUnitOfWork _unitOfWork;
    private readonly ILogger<InsertForecastsHandler> _logger;

    public InsertForecastsHandler(
        ILogger<InsertForecastsHandler> logger,
        IPlutusUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public override async Task Handle(
        InsertForecastsInput command,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle insert forecasts command '{@command}'.", command);
        cancellationToken.ThrowIfCancellationRequested();

        await _unitOfWork.Forecasts.Delete(_ => true, cancellationToken);
        await _unitOfWork.Forecasts.CreateMany(command.Forecasts, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogDebug("Successfully handled insert forecasts command.");
    }
}