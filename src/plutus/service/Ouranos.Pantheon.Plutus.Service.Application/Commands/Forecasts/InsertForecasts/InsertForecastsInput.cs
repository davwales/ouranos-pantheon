using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;

namespace Ouranos.Pantheon.Plutus.Service.Application.Commands.Forecasts.InsertForecasts;

public sealed record InsertForecastsInput(
    IReadOnlyList<Forecast> Forecasts
) : ICommand;