using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Talos.Olympus.Service.Aphrodite.Application.Interfaces.Conversations;
using Talos.Olympus.Service.Aphrodite.Infra.TalosMl.Conversations;

namespace Talos.Olympus.Service.Aphrodite.Infra.TalosMl;

public static class TalosMachineLearningModule
{
    public static IServiceCollection AddTalosMachineLearningModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<ITalosMachineLearningClient, TalosMachineLearningClient>(client =>
        {
            var url = configuration.GetValue<string?>("TALOS_ML_URL")
                ?? throw new InvalidOperationException("Invalid Talos Machine Learning URL.");
            
            client.BaseAddress = new Uri(url);
        });

        return services.AddScoped<IGenerateCompletion, GenerateCompletion>();
    }
}