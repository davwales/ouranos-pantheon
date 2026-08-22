using JasperFx.Events.Projections;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Shared.Contract;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;

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
                    options.Projections.Snapshot<Recipe>(SnapshotLifecycle.Inline);
                },
                initialData: [new HestiaRecipeSeedData()]
            );

        return builder;
    }

    public async Task<IHost> Configure(IHost host)
    {
        await host.Services.ApplyCorePostgresMigrations<HestiaDbContext>();
        return host;
    }

    public void MapEndpoints(WebApplication app)
    {
        GetAllRecipesEndpoint.Map(app);
        CreateRecipeEndpoint.Map(app);
        GetRecipeEndpoint.Map(app);
        UpdateRecipeEndpoint.Map(app);
    }
}
