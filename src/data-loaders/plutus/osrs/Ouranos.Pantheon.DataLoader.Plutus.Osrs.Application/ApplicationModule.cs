using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application;

namespace Ouranos.Pantheon.DataLoader.Plutus.Osrs.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services
    )
    {
        return services.AddCoreApplicationModule();
    }

    public static IMediatorRegistrationConfigurator AddModuleConsumers(
        this IMediatorRegistrationConfigurator mediator
    )
    {
        mediator.AddConsumers(typeof(ApplicationModule).Assembly);
        return mediator;
    }
}