using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Ouranos.Pantheon.Plutus.Service.Infra.Postgres;

namespace Ouranos.Pantheon.Plutus.DataLoader.Seed.Extensions;

public static class StartupExtensions
{
    public static IServiceProvider GetServices()
    {
        var configuration = BuildConfiguration();

        return new ServiceCollection()
            .AddLogging(x => x.AddConsole())
            .AddPostgresModule(configuration)
            .BuildServiceProvider();
    }

    private static IConfiguration BuildConfiguration()
    {
        const string configurationFile = "appsettings";

        return new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile($"{configurationFile}.json")
            .AddJsonFile($"{configurationFile}.Development.json", true)
            .Build();
    }
}