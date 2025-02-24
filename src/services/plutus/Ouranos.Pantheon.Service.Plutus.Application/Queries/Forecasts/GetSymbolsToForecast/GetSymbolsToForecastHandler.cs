using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application.Queries.Forecasts.GetSymbolsToForecast;

public sealed class GetSymbolsToForecastHandler
    : QueryHandler<GetSymbolsToForecastInput, WrapperResponse<List<Id<Symbol>>>>
{
    private readonly ILogger<GetSymbolsToForecastHandler> _logger;
    private readonly IRepository<Market> _marketRepository;
    private readonly IQueryExecutor _queryExecutor;
    private readonly IRepository<Symbol> _symbolRepository;

    public GetSymbolsToForecastHandler(
        ILogger<GetSymbolsToForecastHandler> logger,
        IQueryExecutor queryExecutor,
        IRepository<Market> marketRepository,
        IRepository<Symbol> symbolRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(queryExecutor);
        Guard.Against.Null(marketRepository);
        Guard.Against.Null(symbolRepository);

        _logger = logger;
        _queryExecutor = queryExecutor;
        _marketRepository = marketRepository;
        _symbolRepository = symbolRepository;
    }

    public override async Task<WrapperResponse<List<Id<Symbol>>>> Handle(
        GetSymbolsToForecastInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbols to forecast query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var marketIds = await GetMarkets(cancellationToken);
        var symbolIds = await GetSymbols(marketIds, cancellationToken);
        var response = new WrapperResponse<List<Id<Symbol>>>(symbolIds);

        _logger.LogDebug("Successfully handled get symbols to forecast query.");
        return response;
    }

    private async Task<List<Id<Market>>> GetMarkets(CancellationToken cancellationToken)
    {
        var marketsQuery = _marketRepository.AsQueryable(cancellationToken)
            .Where(m => m.IsForecastingEnabled)
            .Select(m => m.Id);

        return await _queryExecutor.ToList(marketsQuery, cancellationToken);
    }

    private async Task<List<Id<Symbol>>> GetSymbols(
        IReadOnlyList<Id<Market>> marketIds,
        CancellationToken cancellationToken
    )
    {
        var symbolsQuery = _symbolRepository.AsQueryable(cancellationToken)
            .Where(s => marketIds.Contains(s.MarketId))
            .Select(s => s.Id);

        return await _queryExecutor.ToList(symbolsQuery, cancellationToken);
    }
}