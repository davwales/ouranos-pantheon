using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.Extensions.Hosting;

namespace Ouranos.Pantheon.Modules.Shared;

public interface IPantheonModule
{
    IHostApplicationBuilder Build(IHostApplicationBuilder builder);

    Task<IHost> Configure(IHost host);

    IRequestExecutorBuilder ConfigureSchema(IRequestExecutorBuilder builder)
    {
        return builder;
    }

    IFilterConventionDescriptor ConfigureSchemaFilters(IFilterConventionDescriptor descriptor)
    {
        return descriptor;
    }

    IMediatorRegistrationConfigurator ConfigureMediator(IMediatorRegistrationConfigurator mediator)
    {
        return mediator;
    }
}