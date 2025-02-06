using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace Ouranos.Pantheon.Tests.Utils.Shouldly;

[ShouldlyMethods]
public static class ServiceCollectionValidations
{
    public static void ShouldContainService(
        this IServiceCollection services,
        Type type,
        ServiceLifetime lifetime,
        string? customMessage = null
    )
    {
        customMessage ??= $"services should contain type '{type}' with lifetime '{lifetime}' but does not";
        services.ShouldContain(s => s.ServiceType == type && s.Lifetime == lifetime, customMessage);
    }
}