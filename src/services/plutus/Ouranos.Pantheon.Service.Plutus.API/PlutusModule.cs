using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Core.API.Interfaces;
using Ouranos.Pantheon.Service.Plutus.API.Jobs;
using Ouranos.Pantheon.Service.Plutus.Application;
using Ouranos.Pantheon.Service.Plutus.Domain.Forecasts;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Recipes;
using Ouranos.Pantheon.Service.Plutus.Domain.SymbolGroups;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Infra.Mongo;
using Ouranos.Pantheon.Service.Plutus.Infra.OuranosMl;

namespace Ouranos.Pantheon.Service.Plutus.API;

public sealed class PlutusModule : IOuranosModule
{
    public IRequestExecutorBuilder ConfigureSchema(IRequestExecutorBuilder builder)
    {
        return builder
            .BindModelId<Market>()
            .BindModelId<Symbol>()
            .BindModelId<SymbolGroup>()
            .BindModelId<Recipe>()
            .BindModelId<Forecast>();
    }

    public IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddApplicationModule(configuration)
            .AddMongoModule(configuration)
            .AddOuranosMachineLearningModule(configuration)
            .AddHostedService<ForecastCreatorJob>();
    }

    public IMediatorRegistrationConfigurator ConfigureMediator(IMediatorRegistrationConfigurator mediator)
    {
        return mediator.AddModuleConsumers();
    }
}