using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Common;
using Ouranos.Pantheon.Modules.Shared.Contract.Domain;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Forecasts.GetForecastEfficacy;

public sealed class GetForecastEfficacyHandler
    : IPantheonHandler<GetForecastEfficacyInput, PagedResponse<GetForecastEfficacyResponse>>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetForecastEfficacyHandler> _logger;
    private readonly IOptions<QueryOptions> _queryOptions;

    public GetForecastEfficacyHandler(
        ILogger<GetForecastEfficacyHandler> logger,
        PlutusDbContext dbContext,
        IOptions<QueryOptions> queryOptions
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);
        Guard.Against.Null(queryOptions);

        _logger = logger;
        _dbContext = dbContext;
        _queryOptions = queryOptions;
    }

    public async Task<PagedResponse<GetForecastEfficacyResponse>> Handle(
        GetForecastEfficacyInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get forecast efficacy query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var limits = _queryOptions.Value;
        Guard.Against.OutOfRange(input.Skip, nameof(input.Skip), 0, limits.MaxSkip);
        Guard.Against.OutOfRange(
            input.Take,
            nameof(input.Take),
            limits.MinPageSize,
            limits.MaxPageSize
        );

        var q = _dbContext
            .ForecastRecordsWithActuals.AsNoTracking()
            .Where(r => r.ActualAveragePrice != null && r.ActualAveragePrice != 0);

        if (input.SymbolId is not null)
        {
            q = q.Where(r => r.SymbolId == (Id<Symbol>)input.SymbolId);
        }

        if (input.MarketId is not null)
        {
            q = q.Where(r => r.MarketId == (Id<Market>)input.MarketId);
        }

        if (input.ModelName is not null)
        {
            q = q.Where(r => r.ModelName == input.ModelName);
        }

        if (input.HorizonDays is not null)
        {
            q = q.Where(r => r.HorizonDays == input.HorizonDays);
        }

        if (input.Since is not null)
        {
            q = q.Where(r => r.GeneratedAt >= input.Since);
        }

        if (input.Until is not null)
        {
            q = q.Where(r => r.GeneratedAt <= input.Until);
        }

        var projected =
            from r in q
            join s in _dbContext.Symbols on r.SymbolId equals s.Id
            select new
            {
                r.SymbolId,
                SymbolName = s.Name,
                r.MarketId,
                r.ModelName,
                r.HorizonDays,
                r.GeneratedAt,
                PredictedAvg = r.PredictedAveragePrice,
                ActualAvg = r.ActualAveragePrice ?? 0m,
            };

        var totalCount = await projected
            .Select(r => new
            {
                r.SymbolId,
                r.ModelName,
                r.HorizonDays,
            })
            .Distinct()
            .CountAsync(cancellationToken);

        var items = await projected
            .GroupBy(r => new
            {
                r.SymbolId,
                r.SymbolName,
                r.MarketId,
                r.ModelName,
                r.HorizonDays,
            })
            .OrderBy(g => g.Key.SymbolId)
            .ThenBy(g => g.Key.ModelName)
            .ThenBy(g => g.Key.HorizonDays)
            .Select(g => new GetForecastEfficacyResponse(
                g.Key.SymbolId,
                g.Key.SymbolName,
                g.Key.MarketId,
                g.Key.ModelName,
                g.Key.HorizonDays,
                g.Count(),
                g.Average(r => Math.Abs(r.PredictedAvg - r.ActualAvg)),
                g.Average(r =>
                    r.ActualAvg == 0 ? 0 : Math.Abs(r.PredictedAvg - r.ActualAvg) / r.ActualAvg
                ),
                g.Average(r => r.PredictedAvg - r.ActualAvg),
                g.Min(r => r.GeneratedAt),
                g.Max(r => r.GeneratedAt)
            ))
            .Skip(input.Skip)
            .Take(input.Take)
            .ToListAsync(cancellationToken);

        _logger.LogDebug("Successfully handled get forecast efficacy request.");
        return new PagedResponse<GetForecastEfficacyResponse>(
            items,
            totalCount,
            input.Skip,
            input.Take
        );
    }
}
