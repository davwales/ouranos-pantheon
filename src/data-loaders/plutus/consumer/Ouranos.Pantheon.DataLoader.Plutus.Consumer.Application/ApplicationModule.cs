using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(this IServiceCollection services)
    {
        return services
            .AddMediatR(cfg => cfg.RegisterServicesFromAssembly(typeof(ApplicationModule).Assembly));
    }
}