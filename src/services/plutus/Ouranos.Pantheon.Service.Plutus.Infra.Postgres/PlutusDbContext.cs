using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;
using Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres;

public sealed class PlutusDbContext(DbContextOptions<PlutusDbContext> options) : OuranosDbContext(options, "plutus")
{
    public DbSet<Forecast> Forecasts { get; set; }

    public DbSet<Market> Markets { get; set; }

    public DbSet<Recipe> Recipes { get; set; }

    public DbSet<SymbolGroup> SymbolGroups { get; set; }

    public DbSet<Symbol> Symbols { get; set; }

    public DbSet<Trade> Trades { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Forecast>();
        modelBuilder.Entity<Market>();
        modelBuilder.Entity<Recipe>();
        modelBuilder.Entity<SymbolGroup>();
        modelBuilder.Entity<Symbol>();
        modelBuilder.Entity<Trade>();
    }
}