using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetAllForecasts.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.GetAllForecasts;

public sealed class GetAllForecastsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllForecastsInput(Take: 10);
        var expected = new PagedResponse<GetAllForecastsResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetAllForecastsResponse>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllForecastsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetAllForecastsResponse>>>();
        await _bus.Received(1).InvokeAsync<PagedResponse<GetAllForecastsResponse>>(input, ct);
    }
}
