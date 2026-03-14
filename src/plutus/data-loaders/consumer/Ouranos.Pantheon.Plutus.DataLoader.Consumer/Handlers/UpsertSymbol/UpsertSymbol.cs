using Ardalis.GuardClauses;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;

namespace Ouranos.Pantheon.Plutus.DataLoader.Consumer.Handlers.UpsertSymbol;

public sealed class UpsertSymbol : IUpsertSymbol
{
    private readonly ILogger<UpsertSymbol> _logger;
    private readonly IPlutusUnitOfWork _unitOfWork;

    public UpsertSymbol(
        ILogger<UpsertSymbol> logger,
        IPlutusUnitOfWork unitOfWork
    )
    {
        Guard.Against.Null(logger);
        Guard.Against.Null(unitOfWork);

        _logger = logger;
        _unitOfWork = unitOfWork;
    }

    public async Task<Symbol> UpsertSymbolAsync(
        UpsertSymbolInput input,
        CancellationToken cancellationToken = default
    )
    {
        _logger.LogTrace("Attempting to upsert symbol with input '{@input}'.", input);
        cancellationToken.ThrowIfCancellationRequested();

        var existingSymbol = await _unitOfWork.Symbols.FirstOrDefault(
            s => s.MarketId == input.MarketId &&
                 s.Code == input.SymbolCode &&
                 s.Subcode == input.SymbolSubcode,
            cancellationToken
        );

        if (existingSymbol is not null)
        {
            existingSymbol.Update(input.SymbolName, input.AdditionalFields);
            await _unitOfWork.Symbols.Update(existingSymbol, cancellationToken);
            await _unitOfWork.SaveChanges(cancellationToken);
            _logger.LogDebug("Successfully updated symbol '{symbolId}'.", existingSymbol.Id);
            return existingSymbol;
        }

        var market = await _unitOfWork.Markets.Read(input.MarketId, cancellationToken);

        var newSymbol = Symbol.Create(
            _unitOfWork.Symbols.CreateId(),
            input.SymbolCode,
            input.SymbolSubcode,
            input.SymbolName,
            market,
            input.AdditionalFields
        );

        await _unitOfWork.Symbols.Create(newSymbol, cancellationToken);
        await _unitOfWork.SaveChanges(cancellationToken);

        _logger.LogDebug("Successfully inserted new symbol '{symbolId}'.", newSymbol.Id);
        return newSymbol;
    }
}