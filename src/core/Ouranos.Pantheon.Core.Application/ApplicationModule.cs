using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ouranos.Pantheon.Core.Application.Mediator;

namespace Ouranos.Pantheon.Core.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddCoreApplicationModule(
        this IServiceCollection services
    )
    {
        services.TryAddTransient<IDispatcher, Dispatcher>();
        return services;
    }
}