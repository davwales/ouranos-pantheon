using System.ClientModel;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;

namespace Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;

public static class OuranosMachineLearningModule
{
    public static IServiceCollection AddCoreOuranosMachineLearningModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<OuranosMachineLearningOptions>(
            configuration.GetSection(OuranosMachineLearningOptions.SectionName)
        );

        services.AddSingleton<OpenAIClient>(sp =>
            {
                var opts = sp.GetRequiredService<IOptions<OuranosMachineLearningOptions>>().Value;
                if (string.IsNullOrWhiteSpace(opts.ConnectionString))
                {
                    throw new InvalidOperationException("Invalid Ouranos Machine Learning URL.");
                }

                return new OpenAIClient(
                    new ApiKeyCredential(opts.ApiKey),
                    new OpenAIClientOptions { Endpoint = new Uri(opts.ConnectionString) }
                );
            }
        );

        services.AddHttpClient<IOuranosMachineLearningClient, OuranosMachineLearningClient>((sp, client) =>
            {
                var opts = sp.GetRequiredService<IOptions<OuranosMachineLearningOptions>>().Value;
                if (string.IsNullOrWhiteSpace(opts.ConnectionString))
                {
                    throw new InvalidOperationException("Invalid Ouranos Machine Learning URL.");
                }

                client.BaseAddress = new Uri(opts.ConnectionString);
            }
        );

        return services;
    }
}
