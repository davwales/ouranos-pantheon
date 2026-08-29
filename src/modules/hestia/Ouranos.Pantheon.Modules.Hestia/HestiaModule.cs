using JasperFx.Events.Projections;
using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Security.AntiSSRF;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.CreateRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetAllRecipes;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.GetRecipeHistory;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Extraction;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ImportRecipe.Scraping;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.ReimportRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.RevertRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.Recipes.UpdateRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.AddManualItem;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.DeleteManualItem;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.GetShoppingList;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.ToggleRecipe;
using Ouranos.Pantheon.Modules.Hestia.Features.ShoppingLists.UpdateCheckedItems;
using Ouranos.Pantheon.Modules.Hestia.Shared;
using Ouranos.Pantheon.Modules.Hestia.Shared.Database;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Hestia.Shared.Domain.Recipes.Events;
using Ouranos.Pantheon.Modules.Shared.Contract;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;
using Wolverine;
using Wolverine.RabbitMQ;

namespace Ouranos.Pantheon.Modules.Hestia;

public sealed class HestiaModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        builder
            .Services.AddCoreOuranosMachineLearningModule(builder.Configuration)
            .Configure<HestiaOptions>(builder.Configuration.GetSection(HestiaOptions.SectionName))
            .AddScoped<IRecipeExtractor, RecipeExtractor>();

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

        builder
            .Services.AddHttpClient<IRecipeScraper, RecipeScraper>(static client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("OuranosHestiaRecipeImporter/1.0");
                client.DefaultRequestHeaders.Accept.ParseAdd("text/html, application/xhtml+xml");
            })
            .ConfigurePrimaryHttpMessageHandler(static () =>
            {
                var policy = new AntiSSRFPolicy(PolicyConfigOptions.ExternalOnlyLatest);
                var handler = policy.GetHandler();
                handler.MaxAutomaticRedirections = 5;
                return handler;
            });

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
        GetRecipeHistoryEndpoint.Map(app);
        RevertRecipeEndpoint.Map(app);
        ImportRecipeEndpoint.Map(app);
        ReimportRecipeEndpoint.Map(app);
        GetShoppingListEndpoint.Map(app);
        ToggleRecipeEndpoint.Map(app);
        AddManualItemEndpoint.Map(app);
        DeleteManualItemEndpoint.Map(app);
        UpdateCheckedItemsEndpoint.Map(app);
    }

    public void ConfigureWolverine(WolverineOptions opts, IConfiguration configuration)
    {
        // IRecipeScraper is registered via AddHttpClient<TInterface,TImpl>(lambda),
        // which is an opaque factory Wolverine 6 cannot inline. Allowlist it for service location.
        opts.CodeGeneration.AlwaysUseServiceLocationFor<IRecipeScraper>();

        opts.PublishMessage<ImportRecipeRequested>()
            .ToRabbitExchange(
                ImportRecipeRequested.Exchange,
                e =>
                {
                    e.BindQueue(ImportRecipeRequested.Queue);
                }
            );

        opts.ListenToRabbitQueue(ImportRecipeRequested.Queue)
            .DeadLetterQueueing(new DeadLetterQueue(ImportRecipeRequested.DeadLetterQueue));
    }
}
