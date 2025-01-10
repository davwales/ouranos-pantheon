using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Infra.RabbitMq;

namespace Ouranos.Pantheon.DataLoader.Plutus.Consumer.Infra.RabbitMq;

public static class RabbitMqModule
{
    public static IServiceCollection AddRabbitMqModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? busRegistrationConfigurator = null
    )
    {
        return services.AddCoreRabbitMqModule(configuration, busRegistrationConfigurator);
    }
}