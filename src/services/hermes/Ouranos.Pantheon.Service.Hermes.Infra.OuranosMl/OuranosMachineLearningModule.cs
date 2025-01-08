using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Service.Hermes.Application.Interfaces.Conversations;
using Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl.Conversations;

namespace Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl;

public static class OuranosMachineLearningModule
{
    public static IServiceCollection AddOuranosMachineLearningModule(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddHttpClient<IOuranosMachineLearningClient, OuranosMachineLearningClient>(client =>
        {
            var url = configuration.GetValue<string?>("Hermes:OuranosMl:ConnectionString")
                      ?? throw new InvalidOperationException("Invalid Ouranos Machine Learning URL.");

            client.BaseAddress = new Uri(url);
        });

        return services.AddScoped<IGenerateCompletion, GenerateCompletion>();
    }
}