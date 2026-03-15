using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Core.API.Interfaces;
using Ouranos.Pantheon.Core.Application;
using Ouranos.Pantheon.Core.Infra.OuranosMl;
using Ouranos.Pantheon.Core.Infra.Postgres;
using Ouranos.Pantheon.Modules.Hermes.Shared.Domain.Assistants;
using Ouranos.Pantheon.Modules.Hermes.Shared.Database;
using Ouranos.Pantheon.Modules.Shared;

namespace Ouranos.Pantheon.Modules.Hermes;

public sealed class HermesModule : IPantheonModule, IOuranosModule
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

    public IRequestExecutorBuilder ConfigureSchema(IRequestExecutorBuilder builder)
    {
        return builder.BindModelId<Assistant>();
    }

    public IFilterConventionDescriptor ConfigureSchemaFilters(IFilterConventionDescriptor descriptor)
    {
        return descriptor.BindModelIdFilter<Assistant>();
    }

    public IMediatorRegistrationConfigurator ConfigureMediator(IMediatorRegistrationConfigurator mediator)
    {
        mediator.AddConsumers(typeof(HermesModule).Assembly);
        return mediator;
    }

    public IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        // TODO: Consolidate this into the `Build` method once IOuranosModule can be removed.

        return services
            .AddCoreApplicationModule()
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