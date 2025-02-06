using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Core.Infra.Mongo;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;

namespace Ouranos.Pantheon.DataLoader.Plutus.Migration.Extensions;

public static class StartupExtensions
{
    public static IServiceProvider GetServices()
    {
        var configuration = BuildConfiguration();
        return new ServiceCollection()
            .AddSingleton<IMigration, Migration>()
            .AddLogging(x => x.AddConsole())
            .AddCoreMongo(configuration)
            .AddPlutusDataLoaderRabbitMqModule(configuration)
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