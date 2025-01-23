using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Actions.ConvertTrade;
using Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Actions.GetTrades;

namespace Ouranos.Pantheon.DataLoader.Plutus.TalosMigration.Extensions;

public static class HostingExtensions
{
    public static IHost ConfigureBuilder(this IHostBuilder builder)
    {
        var configuration = BuildConfiguration();

        builder.ConfigureServices((_, services) => services
            .AddHostedService<TalosProducer>()
            .AddSingleton<IGetTradesAction, GetTradesAction>()
            .AddSingleton<IConvertTradeAction, ConvertTradeAction>()
            .AddCoreMongo(configuration)
            .AddPlutusDataLoaderRabbitMqModule(configuration)
        );

        return builder.Build();
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