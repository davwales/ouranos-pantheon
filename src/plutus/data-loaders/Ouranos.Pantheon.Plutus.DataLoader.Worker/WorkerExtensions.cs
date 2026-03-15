using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;

namespace Ouranos.Pantheon.Plutus.DataLoader.Worker;

public static class WorkerExtensions
{
    public static IServiceCollection ConfigureWorker(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddLogging(x =>
                {
                    var logger = new LoggerConfiguration()
                        .ReadFrom.Configuration(configuration)
                        .CreateLogger();

                    x.AddSerilog(logger);
                }
            );
    }
}