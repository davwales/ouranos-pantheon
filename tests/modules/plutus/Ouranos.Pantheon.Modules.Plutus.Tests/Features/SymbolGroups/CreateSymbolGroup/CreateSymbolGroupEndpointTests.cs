using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.CreateSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.CreateSymbolGroup;

public sealed class CreateSymbolGroupEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnCreated()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new CreateSymbolGroupInput(
            new Id<Market>(Guid.NewGuid().ToString()),
            "My Group",
            null
        );
        var expected = new IdResponse<SymbolGroup>(new Id<SymbolGroup>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<IdResponse<SymbolGroup>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await CreateSymbolGroupEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Created<IdResponse<SymbolGroup>>>();
        await _bus.Received(1).InvokeAsync<IdResponse<SymbolGroup>>(input, ct);
    }
}
