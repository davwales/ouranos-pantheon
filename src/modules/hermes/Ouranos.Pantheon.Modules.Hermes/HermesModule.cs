using Microsoft.AspNetCore.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.Tools.GenerateImage;
using Ouranos.Pantheon.Modules.Hermes.Features.Assistants.ToolSources;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CompactConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.CreateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.DeleteConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GenerateCompletion.Schemas;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetAllConversations;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.GetConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Conversations.UpdateConversation;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.CreateFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.DeleteFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetAllFolders;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.GetFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Folders.UpdateFolder;
using Ouranos.Pantheon.Modules.Hermes.Features.Images.GetImage;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.CreateLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.DeleteLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetAllLoras;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.GetLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Loras.UpdateLora;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.CreateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.DeleteModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAllModels;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetAvailableModels;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.GetModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Models.UpdateModel;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.CreatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.DeletePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetAllPersonas;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.GetPersona;
using Ouranos.Pantheon.Modules.Hermes.Features.Personas.UpdatePersona;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.CreateToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.DeleteToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAllToolSets;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetAvailableTools;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.GetToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.ToolSets.UpdateToolSet;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.CreateTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.DeleteTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetAllTraits;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.GetTrait;
using Ouranos.Pantheon.Modules.Hermes.Features.Traits.UpdateTrait;
using Ouranos.Pantheon.Modules.Hermes.Shared;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared.Contract;
using Ouranos.Pantheon.Modules.Shared.Contract.Application.Assistants;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Contract.Infra.Postgres;
using Wolverine;

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

        ConfigureAssistantTools(builder);

        return builder;
    }

    private static void ConfigureAssistantTools(IHostApplicationBuilder builder)
    {
        builder.Services.AddAssistantTool<HermesChatContext, GenerateImageTool>();
        builder.Services.AddScoped<IHermesToolSource, NativeToolSource>();
        builder.Services.AddScoped<ToolSetResolver>();
    }

    public async Task<IHost> Configure(IHost host)
    {
        await host.Services.ApplyCorePostgresMigrations<HermesDbContext>();
        return host;
    }

    public void ConfigureWolverine(WolverineOptions opts, IConfiguration configuration)
    {
        opts.CodeGeneration.AlwaysUseServiceLocationFor<HermesDbContext>();
    }

    public void MapEndpoints(WebApplication app)
    {
        GetAllPersonasEndpoint.Map(app);
        GetPersonaEndpoint.Map(app);
        CreatePersonaEndpoint.Map(app);
        UpdatePersonaEndpoint.Map(app);
        DeletePersonaEndpoint.Map(app);

        GetAllModelsEndpoint.Map(app);
        GetAvailableModelsEndpoint.Map(app);
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
        GetImageEndpoint.Map(app);

        GetAllTraitsEndpoint.Map(app);
        GetTraitEndpoint.Map(app);
        CreateTraitEndpoint.Map(app);
        UpdateTraitEndpoint.Map(app);
        DeleteTraitEndpoint.Map(app);

        GetAllLorasEndpoint.Map(app);
        GetLoraEndpoint.Map(app);
        CreateLoraEndpoint.Map(app);
        UpdateLoraEndpoint.Map(app);
        DeleteLoraEndpoint.Map(app);

        GetAvailableToolsEndpoint.Map(app);
        GetAllToolSetsEndpoint.Map(app);
        GetToolSetEndpoint.Map(app);
        CreateToolSetEndpoint.Map(app);
        UpdateToolSetEndpoint.Map(app);
        DeleteToolSetEndpoint.Map(app);

        GetAllFoldersEndpoint.Map(app);
        GetFolderEndpoint.Map(app);
        CreateFolderEndpoint.Map(app);
        UpdateFolderEndpoint.Map(app);
        DeleteFolderEndpoint.Map(app);
    }
}
