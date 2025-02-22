using MassTransit;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.Application;
using Ouranos.Pantheon.Core.Application.Mediator;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;
using Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;

namespace Ouranos.Pantheon.Service.Plutus.Application;

public static class ApplicationModule
{
    public static IServiceCollection AddApplicationModule(this IServiceCollection services)
    {
        return services.AddCoreApplicationModule();
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

        return mediator;
    }
}