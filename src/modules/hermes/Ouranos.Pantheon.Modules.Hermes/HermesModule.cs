using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;

namespace Ouranos.Pantheon.Modules.Hermes;

public sealed class HermesModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        builder
            .Services.Configure<HermesOptions>(
                builder.Configuration.GetSection(HermesOptions.SectionName)
            )
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
        CompactConversationEndpoint.Map(app);
        GetAllConversationsEndpoint.Map(app);
        GetConversationEndpoint.Map(app);
        CreateConversationEndpoint.Map(app);
        UpdateConversationEndpoint.Map(app);
        DeleteConversationEndpoint.Map(app);

        GetAllTraitsEndpoint.Map(app);
        GetTraitEndpoint.Map(app);
        CreateTraitEndpoint.Map(app);
        UpdateTraitEndpoint.Map(app);
        DeleteTraitEndpoint.Map(app);
    }
}
