using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;
using Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;
using Ouranos.Pantheon.Service.Plutus.Infra.Postgres.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Infra.Postgres.Trades;

namespace Ouranos.Pantheon.Service.Plutus.Infra.Postgres;

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