using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests;
using Ouranos.Pantheon.Modules.Plutus.Features.Strategies.GetActiveBacktests.Schemas;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Strategies.GetActiveBacktests;

public sealed class GetActiveBacktestsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetActiveBacktestsInput();
        var expected = new PagedResponse<GetActiveBacktestsResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetActiveBacktestsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetActiveBacktestsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetActiveBacktestsResponse>>>();
        await _bus.Received(1).InvokeAsync<PagedResponse<GetActiveBacktestsResponse>>(input, ct);
    }
}
