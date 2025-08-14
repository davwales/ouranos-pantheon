using Ardalis.GuardClauses;
using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.UpsertSymbol;

public sealed class UpsertSymbol : IUpsertSymbol
{
    private readonly ILogger<UpsertSymbol> _logger;
    private readonly IRepository<Symbol> _symbolRepository;

    public UpsertSymbol(
        ILogger<UpsertSymbol> logger,
        IRepository<Symbol> symbolRepository
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(symbolRepository);

        _logger = logger;
        _symbolRepository = symbolRepository;
    }

    public async Task<Symbol> UpsertSymbolAsync(
        UpsertSymbolInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to upsert symbol with input '{@input}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var existingSymbol = await _symbolRepository.FirstOrDefault(
            s => s.MarketId == input.MarketId &&
                 s.Code == input.SymbolCode &&
                 s.Subcode == input.SymbolSubcode,
            cancellationToken
        );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(input.SymbolName, input.AdditionalFields);
            await _symbolRepository.Update(existingSymbol, cancellationToken);
            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            return existingSymbol;
        }

        var newSymbol = new Symbol(
            _symbolRepository.CreateId(),
            input.SymbolCode,
            input.SymbolSubcode,
            input.SymbolName,
            input.MarketId,
            input.AdditionalFields
        );

        await _symbolRepository.Create(newSymbol, cancellationToken);

        _logger.LogDebug("Successfully inserted new symbol '{symbolId}'.", newSymbol.Id);
        return newSymbol;
    }
}