using Ouranos.Pantheon.Modules.Shared.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetAllSymbols.Schemas;

public sealed record GetAllSymbolsInput(
    string? SortField = null,
    string? SortDirection = null,
    int Skip = 0,
    int Take = 10,
    string[]? Filter = null
) : IQuery<PagedResponse<GetAllSymbolsResponse>>;
