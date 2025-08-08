using HotChocolate.Data.Filters;
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
using Ouranos.Pantheon.Service.Plutus.Domain.Trades;
using Ouranos.Pantheon.Service.Plutus.Infra.OuranosMl;
using Ouranos.Pantheon.Service.Plutus.Infra.Postgres;

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