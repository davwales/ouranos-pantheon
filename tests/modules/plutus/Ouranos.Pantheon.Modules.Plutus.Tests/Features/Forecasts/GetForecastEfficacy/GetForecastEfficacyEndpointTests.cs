using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Forecasts.GetForecastEfficacy;

public sealed class GetForecastEfficacyEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetForecastEfficacyInput(Take: 10);
        var expected = new PagedResponse<GetForecastEfficacyResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetForecastEfficacyEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetForecastEfficacyResponse>>>();
        await _bus.Received(1).InvokeAsync<PagedResponse<GetForecastEfficacyResponse>>(input, ct);
    }
}
