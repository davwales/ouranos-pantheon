using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres.Notifications;

namespace Ouranos.Pantheon.Modules.Shared;

public sealed class SharedModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        builder
            .Services.Configure<NotificationOptions>(
                builder.Configuration.GetSection(NotificationOptions.SectionName)
            )
            .AddCorePostgresModule<SharedDbContext>(
                builder.Configuration,
                typeof(SharedModule).Assembly
            )
            .AddSingleton<NotificationSenderJob>();

        return builder;
    }

    public async Task<IHost> Configure(IHost host)
    {
        await host.Services.ApplyCorePostgresMigrations<SharedDbContext>();
        return host;
    }
}
