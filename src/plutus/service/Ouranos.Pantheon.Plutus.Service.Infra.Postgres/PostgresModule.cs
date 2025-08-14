using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Application.Interfaces.Trades;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;
using Ouranos.Pantheon.Plutus.Service.Domain.SymbolGroups;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;
using Ouranos.Pantheon.Plutus.Service.Infra.Postgres.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Infra.Postgres.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Infra.Postgres;

public static class PostgresModule
{
    public static IServiceCollection AddPostgresModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddCorePostgresModule<PlutusDbContext>(
                configuration,
                typeof(PostgresModule).Assembly
            )
            .AddRepository<PlutusDbContext, Forecast>()
            .AddRepository<PlutusDbContext, Market>()
            .AddRepository<PlutusDbContext, Recipe>()
            .AddRepository<PlutusDbContext, SymbolGroup>()
            .AddRepository<PlutusDbContext, Symbol>()
            .AddRepository<PlutusDbContext, Trade>()
            .AddScoped<IBucketTrades, BucketTrades>()
            .AddScoped<IBucketHistoricalData, BucketHistoricalData>();
    }

    public static async Task<IServiceProvider> ApplyPostgresMigrations(
        this IServiceProvider provider
    )
    {
        return await provider.ApplyCorePostgresMigrations<PlutusDbContext>();
    }
}