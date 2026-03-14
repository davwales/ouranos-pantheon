using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Plutus.Service.Application.Options;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;

namespace Ouranos.Pantheon.Plutus.Service.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .Configure<ForecastingOptions>(configuration.GetSection(ForecastingOptions.SectionName))
            .AddCoreApplicationModule();
    }

    public static IMediatorRegistrationConfigurator AddModuleConsumers(
        this IMediatorRegistrationConfigurator mediator
    )
    {
        mediator.AddConsumers(typeof(ApplicationModule).Assembly);
        mediator.AddStandardConsumersForEntity<Market>();
        mediator.AddStandardConsumersForEntity<Symbol>();
        mediator.AddStandardConsumersForEntity<Recipe>();
        mediator.AddStandardConsumersForEntity<Forecast>();
        mediator.AddStandardConsumersForEntity<Trade>();

        return mediator;
    }
}