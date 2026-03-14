using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Core.API.Interfaces;

public interface IOuranosModule
{
    IRequestExecutorBuilder ConfigureSchema(IRequestExecutorBuilder builder);

    IFilterConventionDescriptor ConfigureSchemaFilters(IFilterConventionDescriptor descriptor);

    IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration);

    Task<IServiceProvider> UseModule(IServiceProvider provider);

    IMediatorRegistrationConfigurator ConfigureMediator(IMediatorRegistrationConfigurator mediator);
}