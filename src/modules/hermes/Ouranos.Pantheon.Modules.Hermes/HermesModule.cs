using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAllAssistants;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.GetAssistant;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.CreateAssistant;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.UpdateAssistant;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.DeleteAssistant;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;

namespace Ouranos.Pantheon.Modules.Hermes;

public sealed class HermesModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        ConfigureServices(builder.Services, builder.Configuration);
        return builder;
    }

    public async Task<IHost> Configure(IHost host)
    {
        await UseModule(host.Services);
        return host;
    }

    public void MapEndpoints(WebApplication app)
    {
        GetAllAssistantsEndpoint.Map(app);
        GetAssistantEndpoint.Map(app);
        CreateAssistantEndpoint.Map(app);
        UpdateAssistantEndpoint.Map(app);
        DeleteAssistantEndpoint.Map(app);
        GenerateCompletionEndpoint.Map(app);
    }

    public IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Consolidate this into the `Build` method once IOuranosModule can be removed.

        return services
            .AddCoreOuranosMachineLearningModule(configuration)
            .AddCorePostgresModule<HermesDbContext>(
                configuration,
                typeof(HermesModule).Assembly
            );
    }

    public async Task<IServiceProvider> UseModule(IServiceProvider provider)
    {
        // TODO: Consolidate this into the `Configure` method once IOuranosModule can be removed.

        await provider.ApplyCorePostgresMigrations<HermesDbContext>();
        return provider;
    }
}
