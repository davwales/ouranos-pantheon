using Ardalis.GuardClauses;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;

namespace Ouranos.Pantheon.Plutus.Service.Application.Queries.Symbols.GetDailySymbolSummary;

public sealed class GetDailySymbolSummaryHandler
    : QueryHandler<GetDailySymbolSummaryInput, GetDailySymbolSummaryResponse>
{
    private readonly ILogger<GetDailySymbolSummaryHandler> _logger;
    private readonly IQueryExecutor _queryExecutor;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public GetDailySymbolSummaryHandler(
        ILogger<GetDailySymbolSummaryHandler> logger,
        IPlutusUnitOfWork unitOfWork,
        IQueryExecutor queryExecutor
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);
        Guard.Against.Null(queryExecutor);

        _logger = logger;
        _unitOfWork = unitOfWork;
        _queryExecutor = queryExecutor;
    }

    public override async Task<GetDailySymbolSummaryResponse> Handle(
        GetDailySymbolSummaryInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handler get daily symbol summary query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var today = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);
        var summaryQuery = _unitOfWork.Trades
            .AsQueryable(cancellationToken)
            .Where(t =>
                t.SymbolId == query.SymbolId &&
                t.Timestamp >= today
            )
            .GroupBy(_ => true)
            .Select(g => new
            {
                TotalSpent = g.Sum(x => x.Price * x.Volume),
                MinPrice = g.Min(x => x.Price),
                MaxPrice = g.Max(x => x.Price),
                Volume = g.Sum(x => x.Volume)
            }
            )
            .Where(x => x.Volume > 0)
            .Select(g => new GetDailySymbolSummaryResponse(
                    g.TotalSpent / g.Volume,
                    g.MinPrice,
                    g.MaxPrice,
                    g.Volume
                )
            );

        var response = await _queryExecutor.FirstOrDefault(summaryQuery, cancellationToken)
                       ?? new GetDailySymbolSummaryResponse(0, 0, 0, 0);

        _logger.LogDebug("Successfully handled get daily symbol summary query.");
        return response;
    }
}