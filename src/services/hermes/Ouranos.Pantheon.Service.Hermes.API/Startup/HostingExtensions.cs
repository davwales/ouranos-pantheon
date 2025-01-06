using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Service.Hermes.Application;
using Ouranos.Pantheon.Service.Hermes.Domain.Characters;
using Ouranos.Pantheon.Service.Hermes.Infra.Mongo;
using Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl;

namespace Ouranos.Pantheon.Service.Hermes.API.Startup;

public static class HostingExtensions
{
    public static WebApplication ConfigureBuilder(this WebApplicationBuilder builder)
    {
        builder.Services
            .AddOuranosCore(builder.Configuration, gql => gql
                .ModifyOptions(o => { o.EnableStream = true; })
                .BindModelId<Character>()
            )
            .AddApplicationModule()
            .RegisterMongoBehaviors()
            .AddOuranosMachineLearningModule(builder.Configuration);

        return builder.Build();
    }

    public static WebApplication ConfigureApp(this WebApplication app)
    {
        return app.UseOuranosCore();
    }
}