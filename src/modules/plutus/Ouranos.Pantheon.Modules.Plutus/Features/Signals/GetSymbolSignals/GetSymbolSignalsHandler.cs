using Ardalis.GuardClauses;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals.Schemas;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Shared.Contract.Application;

namespace Ouranos.Pantheon.Modules.Plutus.Features.Signals.GetSymbolSignals;

public sealed class GetSymbolSignalsHandler
    : IPantheonHandler<GetSymbolSignalsInput, GetSymbolSignalsResponse>
{
    private readonly PlutusDbContext _dbContext;
    private readonly ILogger<GetSymbolSignalsHandler> _logger;
    private readonly IReadOnlyDictionary<SignalType, ISignalComputer> _computers;

    public GetSymbolSignalsHandler(
        ILogger<GetSymbolSignalsHandler> logger,
        PlutusDbContext dbContext,
        IEnumerable<ISignalComputer> computers
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(dbContext);

        _logger = logger;
        _dbContext = dbContext;
        _computers = computers.ToDictionary(c => c.Type);
    }

    public async Task<GetSymbolSignalsResponse> Handle(
        GetSymbolSignalsInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to handle get symbol signals query '{@query}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var symbol = await _dbContext
            .Symbols.AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == input.SymbolId, cancellationToken);

        Guard.Against.NotFound(input.SymbolId, symbol);

        var latestRows = await _dbContext
            .LatestSignals.AsNoTracking()
            .Where(s => s.SymbolId == input.SymbolId)
            .ToListAsync(cancellationToken);

        IEnumerable<LatestSignal> filteredRows = latestRows;
        if (input.Intent is { } intent)
        {
            var matchingTypes = _computers
                .Values.Where(c => c.Intents.Any(i => (i & intent) != 0))
                .Select(c => c.Type)
                .ToHashSet();

            filteredRows = latestRows.Where(r => matchingTypes.Contains(r.SignalType));
        }

        var signalResponses = filteredRows
            .Where(r => _computers.ContainsKey(r.SignalType))
            .Select(r => ToSignalResponse(r.SignalType, r.LastValue))
            .ToList();

        _logger.LogDebug("Successfully handled get symbol signals query.");
        return new GetSymbolSignalsResponse(
            symbol.Id,
            symbol.Name,
            signalResponses,
            ComputeSummary(signalResponses)
        );
    }

    private SignalResponse ToSignalResponse(SignalType type, decimal value)
    {
        var computer = _computers[type];
        return new SignalResponse(
            type.ToString(),
            computer.Label,
            computer.Description,
            computer.Intents,
            value,
            DeriveDirection(value),
            DeriveStrength(value)
        );
    }

    private static SignalSummary ComputeSummary(IReadOnlyList<SignalResponse> signals)
    {
        if (signals.Count == 0)
        {
            return new SignalSummary(0m, 0, 0, 0, false, false);
        }

        var avgScore = signals.Average(s => s.Value);
        var bullish = signals.Count(s => s.Value > 0);
        var bearish = signals.Count(s => s.Value < 0);
        var neutral = signals.Count(s => s.Value == 0);

        var flipSignals = signals.Where(s => s.Intents.Contains(InvestmentIntent.Flip)).ToList();
        var isFlipFavourable = flipSignals.Count > 0 && flipSignals.All(s => s.Value > 0);

        var merchSignalTypes = new[]
        {
            nameof(SignalType.TrendMomentum),
            nameof(SignalType.MovingAverageCrossover),
        };
        var isMerchFavourable = signals
            .Where(s => merchSignalTypes.Contains(s.Type))
            .Any(s => s.Value > 0);

        return new SignalSummary(
            avgScore,
            bullish,
            bearish,
            neutral,
            isFlipFavourable,
            isMerchFavourable
        );
    }

    private static SignalDirection DeriveDirection(decimal value)
    {
        return value switch
        {
            > 0 => SignalDirection.Bullish,
            < 0 => SignalDirection.Bearish,
            _ => SignalDirection.Neutral,
        };
    }

    private static SignalStrength DeriveStrength(decimal value)
    {
        var abs = Math.Abs(value);
        return abs switch
        {
            >= 0.67m => SignalStrength.Strong,
            >= 0.33m => SignalStrength.Medium,
            _ => SignalStrength.Weak,
        };
    }
}
