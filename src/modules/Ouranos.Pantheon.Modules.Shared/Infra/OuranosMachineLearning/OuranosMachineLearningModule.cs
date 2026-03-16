using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;

public static class OuranosMachineLearningModule
{
    public static IServiceCollection AddCoreOuranosMachineLearningModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddHttpClient<IOuranosMachineLearningClient, OuranosMachineLearningClient>(client =>
        {
            var url = configuration.GetValue<string?>("Ouranos:OuranosMl:ConnectionString")
                      ?? throw new InvalidOperationException("Invalid Ouranos Machine Learning URL.");

            client.BaseAddress = new Uri(url);
        });

        return services;
    }
}