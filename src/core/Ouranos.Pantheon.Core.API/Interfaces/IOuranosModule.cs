using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Core.API.Interfaces;

public interface IOuranosModule
{
    IRequestExecutorBuilder ConfigureSchema(IRequestExecutorBuilder builder);

    IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration);

    IMediatorRegistrationConfigurator ConfigureMediator(IMediatorRegistrationConfigurator mediator);
}