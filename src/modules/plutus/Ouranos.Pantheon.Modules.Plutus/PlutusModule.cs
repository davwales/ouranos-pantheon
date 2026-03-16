using HotChocolate.Data.Filters;
using HotChocolate.Execution.Configuration;
using MassTransit;
using Microsoft.Extensions.Hosting;
using Ouranos.Pantheon.Modules.Shared.API.Extensions;
using Ouranos.Pantheon.Modules.Shared.Application;
using Ouranos.Pantheon.Modules.Shared.Infra.OuranosMachineLearning;
using Ouranos.Pantheon.Modules.Shared.Infra.Postgres;
using Ouranos.Pantheon.Modules.Plutus.Shared.Database;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Forecasts;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Markets;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Recipes;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Symbols;
using Ouranos.Pantheon.Modules.Plutus.Shared.Domain.Trades;
using Ouranos.Pantheon.Modules.Shared;

namespace Ouranos.Pantheon.Modules.Plutus;

public sealed class PlutusModule : IPantheonModule
{
    public IHostApplicationBuilder Build(IHostApplicationBuilder builder)
    {
        builder.Services
            .AddCoreApplicationModule()
            .AddCoreOuranosMachineLearningModule(builder.Configuration)
            .AddCorePostgresModule<PlutusDbContext>(
                builder.Configuration,
                typeof(PlutusModule).Assembly
            );

        return builder;
    }

    public async Task<IHost> Configure(IHost host)
    {
        await host.Services.ApplyCorePostgresMigrations<PlutusDbContext>();
        return host;
    }

    public IRequestExecutorBuilder ConfigureSchema(IRequestExecutorBuilder builder)
    {
        return builder
            .BindModelId<Forecast>()
            .BindModelId<Market>()
            .BindModelId<Recipe>()
            .BindModelId<Symbol>()
            .BindModelId<Trade>();
    }

    public IFilterConventionDescriptor ConfigureSchemaFilters(IFilterConventionDescriptor descriptor)
    {
        return descriptor
            .BindModelIdFilter<Forecast>()
            .BindModelIdFilter<Market>()
            .BindModelIdFilter<Recipe>()
            .BindModelIdFilter<Symbol>()
            .BindModelIdFilter<Trade>();
    }

    public IMediatorRegistrationConfigurator ConfigureMediator(IMediatorRegistrationConfigurator mediator)
    {
        mediator.AddConsumers(typeof(PlutusModule).Assembly);
        return mediator;
    }
}
