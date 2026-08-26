using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.GetSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.GetSymbolGroup;

public sealed class GetSymbolGroupEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var symbolGroupId = new Id<SymbolGroup>(Guid.NewGuid().ToString());
        var expected = new GetSymbolGroupResponse(
            symbolGroupId,
            new Id<Market>(Guid.NewGuid().ToString()),
            "Group",
            null,
            []
        );

        _bus.InvokeAsync<GetSymbolGroupResponse>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetSymbolGroupEndpoint.Handle(symbolGroupId, TimeFrame.OneDay, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<GetSymbolGroupResponse>>();
        await _bus.Received(1)
            .InvokeAsync<GetSymbolGroupResponse>(Arg.Any<GetSymbolGroupInput>(), ct);
    }
}
