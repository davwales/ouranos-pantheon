using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.DataLoaders;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Signals;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.SymbolGroups;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;

namespace Ouranos.Pantheon.Modules.Plutus.Shared.Database;

public class PlutusDbContext(DbContextOptions<PlutusDbContext> options) : OuranosDbContext(options, "plutus")
{
    public DbSet<OsrsDataLoaderState> OsrsDataLoaderStates { get; set; }

    public DbSet<Forecast> Forecasts { get; set; }

    public DbSet<Market> Markets { get; set; }

    public DbSet<Recipe> Recipes { get; set; }

    public DbSet<Symbol> Symbols { get; set; }

    public DbSet<Trade> Trades { get; set; }

    public DbSet<MarketTradeSnapshot> MarketTradeSnapshots => Set<MarketTradeSnapshot>();

    public DbSet<Signal> Signals => Set<Signal>();

    public DbSet<SymbolGroup> SymbolGroups { get; set; }

    public DbSet<SymbolGroupMember> SymbolGroupMembers { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<OsrsDataLoaderState>();
        modelBuilder.Entity<Forecast>();
        modelBuilder.Entity<Market>();
        modelBuilder.Entity<Recipe>();
        modelBuilder.Entity<Symbol>();
        modelBuilder.Entity<Trade>();
        modelBuilder.Entity<MarketTradeSnapshot>();
        modelBuilder.Entity<Signal>();
        modelBuilder.Entity<SymbolGroup>();
        modelBuilder.Entity<SymbolGroupMember>();
    }
}