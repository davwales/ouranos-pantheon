using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait;

namespace Ouranos.Pantheon.Modules.Hermes;

public sealed class HermesModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddCoreOuranosMachineLearningModule(builder.Configuration)
            .AddCorePostgresModule<HermesDbContext>(
                builder.Configuration,
                typeof(HermesModule).Assembly
            );

        return builder;
    }

    public async Task<IHost> Configure(IHost host)
    {
        await host.Services.ApplyCorePostgresMigrations<HermesDbContext>();
        return host;
    }

    public void MapEndpoints(WebApplication app)
    {
        GetAllPersonasEndpoint.Map(app);
        GetPersonaEndpoint.Map(app);
        CreatePersonaEndpoint.Map(app);
        UpdatePersonaEndpoint.Map(app);
        DeletePersonaEndpoint.Map(app);

        GetAllModelsEndpoint.Map(app);
        GetModelEndpoint.Map(app);
        CreateModelEndpoint.Map(app);
        UpdateModelEndpoint.Map(app);
        DeleteModelEndpoint.Map(app);

        GenerateCompletionEndpoint.Map(app);

        GetAllTraitsEndpoint.Map(app);
        GetTraitEndpoint.Map(app);
        CreateTraitEndpoint.Map(app);
        UpdateTraitEndpoint.Map(app);
        DeleteTraitEndpoint.Map(app);
    }
}
