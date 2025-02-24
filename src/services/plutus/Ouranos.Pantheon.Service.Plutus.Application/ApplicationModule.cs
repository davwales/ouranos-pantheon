using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Application.Options;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;
using Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application;

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
        mediator.AddStandardConsumersForEntity<SymbolGroup>();
        mediator.AddStandardConsumersForEntity<Recipe>();
        mediator.AddStandardConsumersForEntity<Forecast>();

        return mediator;
    }
}