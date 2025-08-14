using Microsoft.EntityFrameworkCore;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;
using Ouranos.Pantheon.Plutus.Service.Domain.SymbolGroups;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres;

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