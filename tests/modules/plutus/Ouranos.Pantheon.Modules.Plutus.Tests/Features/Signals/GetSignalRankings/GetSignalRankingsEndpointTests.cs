using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSignalRankings.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Signals.GetSignalRankings;

public sealed class GetSignalRankingsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetSignalRankingsInput(new Id<Market>(Guid.NewGuid().ToString()), Take: 10);
        var expected = new PagedResponse<GetSignalRankingsResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetSignalRankingsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetSignalRankingsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetSignalRankingsResponse>>>();
        await _bus.Received(1).InvokeAsync<PagedResponse<GetSignalRankingsResponse>>(input, ct);
    }
}
