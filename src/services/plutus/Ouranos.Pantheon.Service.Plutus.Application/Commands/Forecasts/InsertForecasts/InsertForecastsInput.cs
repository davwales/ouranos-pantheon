using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;

namespace Ouranos.Pantheon.Service.Plutus.Application.Commands.Forecasts.InsertForecasts;

public sealed record InsertForecastsInput(
    IReadOnlyList<Forecast> Forecasts
) : ICommand;