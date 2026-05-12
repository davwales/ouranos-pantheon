using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Application;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Symbols.GetDailySymbolSummary;

public sealed class GetDailySymbolSummaryHandler
    : IPantheonHandler<GetDailySymbolSummaryInput, GetDailySymbolSummaryResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetDailySymbolSummaryHandler> _logger;

    public GetDailySymbolSummaryHandler(
        ILogger<GetDailySymbolSummaryHandler> logger,
        PlutusDbContext dbContext
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
    }

    public async Task<GetDailySymbolSummaryResponse> Handle(
        GetDailySymbolSummaryInput query,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handler get daily symbol summary query '{@query}'.", query);
        cancellationToken.ThrowIfCancellationRequested();

        var today = new DateTimeOffset(DateTimeOffset.UtcNow.Date, TimeSpan.Zero);

        var summary = await _dbContext
            .Trades.Where(t => t.SymbolId == query.SymbolId && t.Timestamp >= today)
            .GroupBy(_ => true)
            .Select(g => new GetDailySymbolSummaryResponse(
                g.Sum(x => x.Price * x.Volume) / g.Sum(x => x.Volume),
                g.Min(x => x.Price),
                g.Max(x => x.Price),
                g.Sum(x => x.Volume)
            ))
            .FirstOrDefaultAsync(cancellationToken);

        var response = summary ?? new GetDailySymbolSummaryResponse(0, 0, 0, 0);

        _logger.LogDebug("Successfully handled get daily symbol summary query.");
        return response;
    }
}
