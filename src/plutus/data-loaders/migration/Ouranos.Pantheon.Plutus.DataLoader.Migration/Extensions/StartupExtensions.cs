using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.Plutus.DataLoader.Infra.RabbitMq;
using Ouranos.Pantheon.Plutus.DataLoader.Migration.Migrators;
using Ouranos.Pantheon.Plutus.Service.Infra.Postgres;

namespace Ouranos.Pantheon.Plutus.DataLoader.Migration.Extensions;

public static class StartupExtensions
{
    public static IServiceProvider GetServices()
    {
        var configuration = BuildConfiguration();
        return new ServiceCollection()
            .AddSingleton<IMigration, Migration>()
            .AddTransient<MarketMigrator>()
            .AddTransient<SymbolMigrator>()
            .AddTransient<TradeMigrator>()
            .AddTransient<RecipeMigrator>()
            .AddLogging(x => x
                .AddConsole()
                .AddFilter("Microsoft.EntityFrameworkCore.Database.Command", LogLevel.Warning)
            )
            .AddCoreMongo(configuration)
            .AddPlutusDataLoaderRabbitMqModule(configuration)
            .AddPostgresModule(configuration)
            .BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration()
    {
        const string configurationFile = "appsettings";
        var environment = Environment.GetEnvironmentVariable("DOTNET_ENVIRONMENT") ?? "Production";

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"{configurationFile}.json")
            .AddJsonFile($"{configurationFile}.{environment}.json", true)
            .Build();
    }
}