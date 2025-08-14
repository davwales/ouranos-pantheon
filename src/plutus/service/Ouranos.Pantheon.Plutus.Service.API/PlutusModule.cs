using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Core.API.Interfaces;
using Ouranos.Pantheon.Plutus.Service.API.Jobs;
using Ouranos.Pantheon.Plutus.Service.Application;
using Ouranos.Pantheon.Plutus.Service.Domain.Forecasts;
using Ouranos.Pantheon.Plutus.Service.Domain.Markets;
using Ouranos.Pantheon.Plutus.Service.Domain.Recipes;
using Ouranos.Pantheon.Plutus.Service.Domain.SymbolGroups;
using Ouranos.Pantheon.Plutus.Service.Domain.Symbols;
using Ouranos.Pantheon.Plutus.Service.Domain.Trades;
using Ouranos.Pantheon.Plutus.Service.Infra.OuranosMl;
using Ouranos.Pantheon.Plutus.Service.Infra.Postgres;

namespace Ouranos.Pantheon.Plutus.Service.API;

public sealed class PlutusModule : IOuranosModule
{
    public IRequestExecutorBuilder ConfigureSchema(IRequestExecutorBuilder builder)
    {
        return builder
            .BindModelId<Market>()
            .BindModelId<Symbol>()
            .BindModelId<SymbolGroup>()
            .BindModelId<Recipe>()
            .BindModelId<Forecast>()
            .BindModelId<Trade>();
    }

    public IFilterConventionDescriptor ConfigureSchemaFilters(IFilterConventionDescriptor descriptor)
    {
        return descriptor
            .BindModelIdFilter<Market>()
            .BindModelIdFilter<Symbol>()
            .BindModelIdFilter<SymbolGroup>()
            .BindModelIdFilter<Recipe>()
            .BindModelIdFilter<Forecast>()
            .BindModelIdFilter<Trade>();
    }

    public IServiceCollection ConfigureServices(IServiceCollection services, IConfiguration configuration)
    {
        return services
            .AddApplicationModule(configuration)
            .AddPostgresModule(configuration)
            .AddOuranosMachineLearningModule(configuration)
            .AddHostedService<ForecastCreatorJob>();
    }

    public async Task<IServiceProvider> UseModule(IServiceProvider provider)
    {
        return await provider.ApplyPostgresMigrations();
    }

    public IMediatorRegistrationConfigurator ConfigureMediator(IMediatorRegistrationConfigurator mediator)
    {
        return mediator.AddModuleConsumers();
    }
}