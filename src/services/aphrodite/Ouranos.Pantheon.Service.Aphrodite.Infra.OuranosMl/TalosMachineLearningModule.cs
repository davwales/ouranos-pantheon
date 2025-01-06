using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Service.Aphrodite.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Service.Aphrodite.Infra.OuranosMl.Conversations;

namespace Ouranos.Pantheon.Service.Aphrodite.Infra.OuranosMl;

public static class OuranosMachineLearningModule
{
    public static IServiceCollection AddOuranosMachineLearningModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IOuranosMachineLearningClient, OuranosMachineLearningClient>(client =>
        {
            var url = configuration.GetValue<string?>("TALOS_ML_URL")
                ?? throw new InvalidOperationException("Invalid Ouranos Machine Learning URL.");
            
            client.BaseAddress = new Uri(url);
        });

        return services.AddScoped<IGenerateCompletion, GenerateCompletion>();
    }
}