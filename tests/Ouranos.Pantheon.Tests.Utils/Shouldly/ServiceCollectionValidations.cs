using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Ouranos.Pantheon.Tests.Utils.Shouldly;

[ShouldlyMethods]
public static class ServiceCollectionValidations
{
    public static void ShouldContainService<T>(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        string? customMessage = null
    )
    {
        var type = typeof(T);
        customMessage ??= $"services should contain type '{type}' with lifetime '{lifetime}' but does not";
        services.ShouldContain(s => s.ServiceType == type && s.Lifetime == lifetime, customMessage);
    }

    public static void ShouldContainServiceWithImplementation<TService, TImplementation>(
        this IServiceCollection services,
        ServiceLifetime lifetime,
        string? customMessage = null
    )
    {
        var serviceType = typeof(TService);
        var implementationType = typeof(TImplementation);

        customMessage ??=
            $"services should contain service '{serviceType}' with implementation '{implementationType}' with lifetime '{lifetime}' but does not";

        services.ShouldContain(
            s => s.ServiceType == serviceType && s.ImplementationType == implementationType && s.Lifetime == lifetime,
            customMessage);
    }
}