using System.ClientModel;
using System.ClientModel.Primitives;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using OpenAI;

namespace Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;

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

            return new OpenAIClient(new ApiKeyCredential(opts.ApiKey), CreateClientOptions(opts));
        });

        services.AddHttpClient<IOuranosMachineLearningClient, OuranosMachineLearningClient>(
            ConfigureHttpClient
        );

        return services;
    }

    /// <summary>
    /// Points an <see cref="HttpClient"/> at OuranosMl, for module-specific clients of its
    /// non-OpenAI endpoints.
    /// </summary>
    public static void ConfigureHttpClient(IServiceProvider services, HttpClient client)
    {
        var opts = services.GetRequiredService<IOptions<OuranosMachineLearningOptions>>().Value;
        if (string.IsNullOrWhiteSpace(opts.ConnectionString))
        {
            throw new InvalidOperationException("Invalid Ouranos Machine Learning URL.");
        }

        client.BaseAddress = new Uri(opts.ConnectionString);
    }

    // Completions are not idempotent: the SDK's default retry policy would resend a request the
    // model may already be generating, so failures surface to the caller instead.
    internal static OpenAIClientOptions CreateClientOptions(OuranosMachineLearningOptions options)
    {
        return new OpenAIClientOptions
        {
            Endpoint = new Uri(options.ConnectionString),
            RetryPolicy = new ClientRetryPolicy(maxRetries: 0),
        };
    }
}
