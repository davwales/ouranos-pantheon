using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup;
using Ouranos.Pantheon.Modules.Plutus.Features.SymbolGroups.DeleteSymbolGroup.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.SymbolGroups.DeleteSymbolGroup;

public sealed class DeleteSymbolGroupEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var symbolGroupId = new Id<SymbolGroup>(Guid.NewGuid().ToString());
        var expected = new IdResponse<SymbolGroup>(new Id<SymbolGroup>(Guid.NewGuid().ToString()));

        _bus.InvokeAsync<IdResponse<SymbolGroup>>(Arg.Any<object>(), Arg.Any<CancellationToken>())
            .Returns(Task.FromResult(expected));

        // Act
        var result = await DeleteSymbolGroupEndpoint.Handle(symbolGroupId, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<IdResponse<SymbolGroup>>>();
        await _bus.Received(1)
            .InvokeAsync<IdResponse<SymbolGroup>>(Arg.Any<DeleteSymbolGroupInput>(), ct);
    }
}
