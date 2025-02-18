using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.DataLoader.Plutus.Application.Interfaces.Trades;
using Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq.Trades;

namespace Ouranos.Pantheon.DataLoader.Plutus.Infra.RabbitMq;

public static class RabbitMqModule
{
    public static IServiceCollection AddPlutusDataLoaderRabbitMqModule(
        this IServiceCollection services,
        IConfiguration configuration,
        Action<IBusRegistrationConfigurator>? busRegistrationConfigurator = null
    )
    {
        return services
            .AddSingleton<IQueueTradeMessages, QueueTradeMessages>()
            .AddMassTransit(x =>
            {
                busRegistrationConfigurator?.Invoke(x);
                x.ConfigureRabbitMq(configuration);
            });
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

            if (options.RetryCount.HasValue)
            {
                cfg.UseMessageRetry(r => r.Immediate(options.RetryCount.Value));
            }
        });
    }
}