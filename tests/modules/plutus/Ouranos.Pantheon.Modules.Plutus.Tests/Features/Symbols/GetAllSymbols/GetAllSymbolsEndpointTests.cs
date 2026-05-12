using Microsoft.AspNetCore.Http.HttpResults;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;
using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Wolverine;

namespace Ouranos.Pantheon.Modules.Plutus.Tests.Features.Symbols.GetAllSymbols;

public sealed class GetAllSymbolsEndpointTests
{
    private readonly IMessageBus _bus = Substitute.For<IMessageBus>();

    [Fact]
    public async Task Handle_WhenCalled_ShouldReturnOk()
    {
        // Arrange
        var ct = CancellationToken.None;
        var input = new GetAllSymbolsInput(Take: 10);
        var expected = new PagedResponse<GetAllSymbolsResponse>([], 0, 0, 10);

        _bus.InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(
                Arg.Any<object>(),
                Arg.Any<CancellationToken>()
            )
            .Returns(Task.FromResult(expected));

        // Act
        var result = await GetAllSymbolsEndpoint.Handle(input, _bus, ct);

        // Assert
        result.ShouldBeOfType<Ok<PagedResponse<GetAllSymbolsResponse>>>();
        await _bus.Received(1).InvokeAsync<PagedResponse<GetAllSymbolsResponse>>(input, ct);
    }
}
