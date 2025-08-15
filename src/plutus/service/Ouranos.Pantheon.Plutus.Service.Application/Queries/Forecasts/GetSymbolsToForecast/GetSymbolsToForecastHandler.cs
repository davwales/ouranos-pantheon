using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Common;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Forecasts.GetSymbolsToForecast;

public sealed class GetSymbolsToForecastHandler
    : QueryHandler<GetSymbolsToForecastInput, WrapperResponse<List<Symbol>>>
{
    private readonly ILogger<GetSymbolsToForecastHandler> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public GetSymbolsToForecastHandler(
        ILogger<GetSymbolsToForecastHandler> logger,
        IQueryExecutor queryExecutor,
        IPlutusUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(queryExecutor);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _queryExecutor = queryExecutor;
        _unitOfWork = unitOfWork;
    }

    public override async Task<WrapperResponse<List<Symbol>>> Handle(
        GetSymbolsToForecastInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbols to forecast query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var marketIds = await GetMarkets(cancellationToken);

        if (marketIds.Count == 0)
        {
            _logger.LogDebug("No markets configured with forecasting support.");
            return new WrapperResponse<List<Symbol>>([]);
        }

        var symbols = await GetSymbols(marketIds, cancellationToken);
        var response = new WrapperResponse<List<Symbol>>(symbols);

        _logger.LogDebug("Successfully handled get symbols to forecast query.");
        return response;
    }

    private async Task<List<Id<Market>>> GetMarkets(CancellationToken cancellationToken)
    {
        var marketsQuery = _unitOfWork.Markets.AsQueryable(cancellationToken)
            .Where(m => m.IsForecastingEnabled)
            .Select(m => m.Id);

        return await _queryExecutor.ToList(marketsQuery, cancellationToken);
    }

    private async Task<List<Symbol>> GetSymbols(
        IReadOnlyList<Id<Market>> marketIds,
        CancellationToken cancellationToken
    )
    {
        return await _unitOfWork.Symbols.ReadAll(
            s => marketIds.Contains(s.MarketId),
            cancellationToken
        );
    }
}