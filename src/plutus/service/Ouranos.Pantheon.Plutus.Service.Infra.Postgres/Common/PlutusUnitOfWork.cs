using Ouranos.Pantheon.Core.Application.Interfaces.Common;
using Ouranos.Pantheon.Core.Infra.Postgres.Common;
using Ouranos.Pantheon.Plutus.DataLoader.Consumer.Models;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Common;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;
using Ouranos.Pantheon.Plutus.Service.Domain.SymbolGroups;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres.Common;

public sealed class PlutusUnitOfWork : UnitOfWork<PlutusDbContext>, IPlutusUnitOfWork
{
    public PlutusUnitOfWork(
        PlutusDbContext context,
        IServiceProvider serviceProvider
    ) : base(context, serviceProvider)
    {
        Forecasts = GetRepository<Forecast>();
        Markets = GetRepository<Market>();
        Recipes = GetRepository<Recipe>();
        SymbolGroups = GetRepository<SymbolGroup>();
        Symbols = GetRepository<Symbol>();
        Trades = GetRepository<Trade>();
        TradeMessages = GetRepository<TradeMessage>();
    }

    public IRepository<Forecast> Forecasts { get; }
    public IRepository<Market> Markets { get; }
    public IRepository<Recipe> Recipes { get; }
    public IRepository<SymbolGroup> SymbolGroups { get; }
    public IRepository<Symbol> Symbols { get; }
    public IRepository<Trade> Trades { get; }
    public IRepository<TradeMessage> TradeMessages { get; }
}