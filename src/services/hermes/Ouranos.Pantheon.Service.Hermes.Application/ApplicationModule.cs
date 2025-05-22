using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Hermes.Domain.Assistants;

namespace Ouranos.Pantheon.Service.Hermes.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(this IServiceCollection services)
    {
        return services.AddCoreApplicationModule();
    }

    public static IMediatorRegistrationConfigurator AddModuleConsumers(
        this IMediatorRegistrationConfigurator mediator
    )
    {
        mediator.AddConsumers(typeof(ApplicationModule).Assembly);
        mediator.AddStandardConsumersForEntity<Assistant>();

        return mediator;
    }
}