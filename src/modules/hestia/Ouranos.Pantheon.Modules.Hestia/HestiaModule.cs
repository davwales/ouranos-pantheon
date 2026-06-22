using JasperFx.Events.Projections;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Projections;
using Ouranos.Pantheon.Modules.Shared;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Hestia;

public sealed class HestiaModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        builder
            .Services.AddCorePostgresModule<HestiaDbContext>(
                builder.Configuration,
                typeof(HestiaModule).Assembly
            )
            .AddCoreMartenModule<IHestiaMartenStore>(
                builder.Configuration,
                HestiaDbContext.SchemaName,
                options =>
                {
                    options.Projections.Add<RecipeProjection>(ProjectionLifecycle.Async);
                }
            );

        return builder;
    }

    public async Task<IHost> Configure(IHost host)
    {
        await host.Services.ApplyCorePostgresMigrations<HestiaDbContext>();
        return host;
    }

    public void MapEndpoints(WebApplication app) { }
}
