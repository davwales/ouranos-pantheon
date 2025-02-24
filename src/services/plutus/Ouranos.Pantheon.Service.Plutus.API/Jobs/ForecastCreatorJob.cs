using Ardalis.GuardClauses;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Common.Extensions;
using Ouranos.Pantheon.Service.Plutus.Application.Commands.Forecasts.InsertForecasts;
using Ouranos.Pantheon.Service.Plutus.Application.Options;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetForecasts;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetHistoricalData;
using Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetSymbolsToForecast;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.API.Jobs;

public sealed class ForecastCreatorJob : BackgroundService
{
    private readonly IDispatcher _dispatcher;
    private readonly ForecastingOptions _forecastingOptions;
    private readonly ILogger<ForecastCreatorJob> _logger;

    private DateTime _lastExecuted = DateTime.MinValue;

    public ForecastCreatorJob(
        ILogger<ForecastCreatorJob> logger,
        IDispatcher dispatcher,
        IOptions<ForecastingOptions> forecastingOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dispatcher);
        Guard.Against.Null(forecastingOptions?.Value);

        _logger = logger;
        _dispatcher = dispatcher;
        _forecastingOptions = forecastingOptions.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken cancellationToken)
    {
        if (!_forecastingOptions.IsEnabled)
        {
            _logger.LogInformation("Forecasting is disabled, exiting job.");
            return;
        }

        while (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogTrace("Attempting to execute forecast creator job.");

            var currentDate = DateTime.UtcNow.Date;
            if (_lastExecuted >= currentDate)
            {
                var delay = _lastExecuted.AddDays(1) - _lastExecuted;
                _logger.LogInformation("Waiting '{seconds}' seconds to generate forecasts.", delay.TotalSeconds);
                await Task.Delay(delay, cancellationToken);
                continue;
            }

            try
            {
                await CreateForecasts(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Failed to generate forecasts.");
                await Task.Delay(TimeSpan.FromMinutes(1), cancellationToken);
                continue;
            }

            _lastExecuted = DateTime.UtcNow.Date;
            _logger.LogInformation("Successfully executed forecast creator job.");
        }
    }

    private async Task CreateForecasts(CancellationToken cancellationToken)
    {
        var symbolsQuery = new GetSymbolsToForecastInput();
        var symbolIds = await _dispatcher.Send(symbolsQuery, cancellationToken);
        if (symbolIds.Value.Count == 0)
        {
            _logger.LogInformation("There are no symbols to forecast.");
            return;
        }

        List<Forecast> forecasts = [];
        foreach (var batch in symbolIds.Value.Batch(_forecastingOptions.BatchSize))
        {
            var batchIds = batch.ToList();

            var historicalDataQuery = new GetHistoricalDataInput(batchIds);
            var historicalData = await _dispatcher.Send(
                historicalDataQuery,
                cancellationToken
            );

            var getForecastsQuery = new GetForecastsInput(historicalData.Value);
            var batchForecasts = await _dispatcher.Send(
                getForecastsQuery,
                cancellationToken
            );

            forecasts.AddRange(batchForecasts.Value);
        }

        var insertForecastsQuery = new InsertForecastsInput(forecasts);
        await _dispatcher.Send(insertForecastsQuery, cancellationToken);
    }
}