using Flagsmith;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Ouranos.Pantheon.Modules.Shared.Infra.Flagsmith;

public static class FlagsmithModule
{
    public static IServiceCollection AddCoreFlagsmithModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .Configure<FlagsmithOptions>(configuration.GetSection(FlagsmithOptions.SectionName))
            .AddSingleton<IFlagsmithClient>(sp =>
                {
                    var opts = sp.GetRequiredService<IOptions<FlagsmithOptions>>().Value;
                    return new FlagsmithClient(
                        new FlagsmithConfiguration
                        {
                            ApiUri = new Uri(opts.ApiUrl),
                            EnvironmentKey = opts.EnvironmentKey
                        }
                    );
                }
            );
    }
}
