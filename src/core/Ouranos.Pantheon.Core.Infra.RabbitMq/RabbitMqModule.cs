using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Ouranos.Pantheon.Core.Infra.RabbitMq;

public static class RabbitMqModule
{
    public static IServiceCollection AddCoreRabbitMqModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? busRegistrationConfigurator = null
    )
    {
        if (services.All(s => s.ServiceType != typeof(IBus)))
        {
            return services.AddMassTransit(x =>
            {
                busRegistrationConfigurator?.Invoke(x);
                x.ConfigureRabbitMq(configuration);
            });
        }

        using var loggerFactory = LoggerFactory.Create(builder => builder.AddConsole());
        var logger = loggerFactory.CreateLogger(nameof(RabbitMqModule));
        logger.LogWarning("MassTransit has already been registered. Skipping registration.");
        return services;
    }

    public static void ConfigureRabbitMq(
        this IBusRegistrationConfigurator configurator,
        IConfiguration configuration
    )
    {
        configurator.UsingRabbitMq((context, cfg) =>
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
    }
}