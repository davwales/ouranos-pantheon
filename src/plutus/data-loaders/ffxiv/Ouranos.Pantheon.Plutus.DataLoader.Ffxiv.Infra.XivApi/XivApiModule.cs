using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Application.Interfaces.Items;
using Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Infra.XivApi.Items;

namespace Ouranos.Pantheon.Plutus.DataLoader.Ffxiv.Infra.XivApi;

public static class XivApiModule
{
    public static IServiceCollection AddXivApiModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        services.Configure<XivApiOptions>(configuration.GetSection(XivApiOptions.SectionName));

        services
            .AddMemoryCache()
            .AddSingleton<IGetItems, GetItems>()
            .AddHttpClient<IGithubClient, GithubClient>((sp, client) =>
                {
                    var options = sp.GetRequiredService<IOptions<XivApiOptions>>();
                    client.BaseAddress = new Uri(options.Value.BaseAddress);
                }
            );

        return services;
    }
}