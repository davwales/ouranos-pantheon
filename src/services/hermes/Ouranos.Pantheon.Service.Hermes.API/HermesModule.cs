using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Core.API.Interfaces;
using Ouranos.Pantheon.Service.Hermes.Application;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;
using Ouranos.Pantheon.Service.Hermes.Infra.OuranosMl;
using Ouranos.Pantheon.Service.Hermes.Infra.Postgres;

namespace Ouranos.Pantheon.Service.Hermes.API;

public sealed class HermesModule : IOuranosModule
{
    public IRequestExecutorBuilder ConfigureSchema(IRequestExecutorBuilder builder)
    {
        return builder.BindModelId<Assistant>();
    }

    public IFilterConventionDescriptor ConfigureSchemaFilters(IFilterConventionDescriptor descriptor)
    {
        return descriptor.BindModelIdFilter<Assistant>();
    }

    public IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddApplicationModule()
            .AddPostgresModule(configuration)
            .AddOuranosMachineLearningModule(configuration);
    }

    public async Task<IServiceProvider> UseModule(IServiceProvider provider)
    {
        return await provider.ApplyPostgresMigrations();
    }

    public IMediatorRegistrationConfigurator ConfigureMediator(IMediatorRegistrationConfigurator mediator)
    {
        return mediator.AddModuleConsumers();
    }
}