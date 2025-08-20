using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;
using Ouranos.Pantheon.Plutus.Service.Domain.SymbolGroups;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;

public interface IPlutusUnitOfWork : IUnitOfWork
{
    IRepository<Forecast> Forecasts { get; }

    IRepository<Market> Markets { get; }

    IRepository<Recipe> Recipes { get; }

    IRepository<SymbolGroup> SymbolGroups { get; }

    IRepository<Symbol> Symbols { get; }

    IRepository<Trade> Trades { get; }

    IRepository<TradeMessage> TradeMessages { get; }
}