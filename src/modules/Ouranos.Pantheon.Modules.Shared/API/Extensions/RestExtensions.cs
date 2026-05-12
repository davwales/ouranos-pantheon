using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Converters;

namespace Ouranos.Pantheon.Modules.Shared.API.Extensions;

public static class RestExtensions
{
    public static IServiceCollection ConfigureRest(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.AddProblemDetails();

        services.ConfigureHttpJsonOptions(options =>
        {
            options.SerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            options.SerializerOptions.Converters.Add(new IdJsonConverterFactory());
            options.SerializerOptions.Converters.Add(new JsonStringEnumConverter());
        });

        return services;
    }
}
