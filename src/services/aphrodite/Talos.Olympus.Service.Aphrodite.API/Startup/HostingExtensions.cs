using Talos.Olympus.Core.API.Extensions;
using Talos.Olympus.Service.Aphrodite.Application;
using Talos.Olympus.Service.Aphrodite.Domain.Characters;
using Talos.Olympus.Service.Aphrodite.Infra.Mongo;
using Talos.Olympus.Service.Aphrodite.Infra.TalosMl;

namespace Talos.Olympus.Service.Aphrodite.API.Startup;

public static class HostingExtensions
{
    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddTalosCore(builder.Configuration, gql => gql
                .ModifyOptions(o => { o.EnableStream = true; })
                .BindModelId<Character>()
            )
            .AddApplicationModule()
            .RegisterMongoBehaviors()
            .AddTalosMachineLearningModule(builder.Configuration);

        return builder.Build();
    }

    public static WebApplication ConfigureApp(this WebApplication app)
    {
        return app.UseTalosCore();
    }
}