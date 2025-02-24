using Ouranos.Pantheon.Core.Infra.OuranosMl.Dtos;

namespace Ouranos.Pantheon.Core.Infra.OuranosMl.Requests;

public sealed record GetPlutusForecastsRequest(
    int NumPredictions,
    List<List<ForecastPoint>> Points
);