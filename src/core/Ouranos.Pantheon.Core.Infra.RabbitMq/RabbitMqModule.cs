using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Ouranos.Pantheon.Core.Infra.RabbitMq;

public static class RabbitMqModule
{
    public static IServiceCollection AddCoreRabbitMqModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? busRegistrationConfigurator = null
    )
    {
        return services
            .AddMassTransit(x =>
            {
                busRegistrationConfigurator?.Invoke(x);

                x.UsingRabbitMq((context, cfg) =>
                {
                    var options = configuration.GetSection(RabbitMqOptions.SectionName).Get<RabbitMqOptions>()
                                  ?? throw new InvalidOperationException("Cannot find RabbitMQ options.");

                    cfg.Host(options.Host, "/", h =>
                    {
                        h.Username(options.Username);
                        h.Password(options.Password);
                    });

                    cfg.ConfigureEndpoints(context);

                    if (options.ConcurrencyLimit.HasValue)
                    {
                        cfg.UseConcurrencyLimit(options.ConcurrencyLimit.Value);
                    }
                });
            });
    }
}