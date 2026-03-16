using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Ouranos.Pantheon.Modules.Shared.Application.Mediator;
using MassTransit.Mediator;

namespace Ouranos.Pantheon.Modules.Shared.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddCoreApplicationModule(
        this IServiceCollection services
    )
    {
        services.TryAddTransient<IDispatcher, Dispatcher<IMediator>>();
        services.TryAddTransient<IScopedDispatcher, Dispatcher<IScopedMediator>>();
        return services;
    }
}