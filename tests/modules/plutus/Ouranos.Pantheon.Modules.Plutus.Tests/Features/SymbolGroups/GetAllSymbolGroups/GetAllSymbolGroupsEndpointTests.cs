using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetAllSymbolGroups.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.GetAllSymbolGroups;

public sealed class GetAllSymbolGroupsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllSymbolGroupsInput(new Id<Market>(Guid.NewGuid().ToString()), Take: 10);
        var expected = new PagedResponse<GetAllSymbolGroupsResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetAllSymbolGroupsResponse>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllSymbolGroupsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetAllSymbolGroupsResponse>>>();
        await _bus.Received(1)
            .InvokeAsync<PagedResponse<GetAllSymbolGroupsResponse>>(input, ct);
    }
}
