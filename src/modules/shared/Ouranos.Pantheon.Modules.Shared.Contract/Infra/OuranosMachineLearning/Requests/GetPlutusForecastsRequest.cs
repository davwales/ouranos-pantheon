using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Dtos;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning.Requests;

public sealed record GetPlutusForecastsRequest(
    int NumPredictions,
    List<List<ForecastPoint>> Points
);
