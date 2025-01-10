using Ouranos.Pantheon.Core.Domain.Common;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application.Interfaces.Symbols;

public interface IGetSymbolByCode
{
    Task<Symbol?> GetSymbolByCodeAsync(
        Id<Market> marketId,
        string code,
        CancellationToken cancellationToken = default
    );
}