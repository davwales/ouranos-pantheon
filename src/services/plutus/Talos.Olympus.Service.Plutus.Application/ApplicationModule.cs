using Microsoft.Extensions.DependencyInjection;

namespace Talos.Olympus.Service.Plutus.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(this IServiceCollection services)
    {
        return services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationModule).Assembly));
    }
}