using HotChocolate.Execution.Configuration;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Ouranos.Pantheon.Core.API.Extensions;
using Ouranos.Pantheon.Service.Plutus.Application;
using Ouranos.Pantheon.Service.Plutus.Domain.Markets;
using Ouranos.Pantheon.Service.Plutus.Domain.Symbols;
using Ouranos.Pantheon.Service.Plutus.Infra.Mongo;

namespace Ouranos.Pantheon.Service.Plutus.API;

public static class PlutusModule
{
    public static IServiceCollection AddPlutusModule(
        this IServiceCollection services,
        IConfiguration configuration
    )
    {
        return services
            .AddApplicationModule()
            .AddMongoModule(configuration);
    }

    public static IRequestExecutorBuilder AddPlutusSchema(this IRequestExecutorBuilder builder)
    {
        return builder
            .BindModelId<Market>()
            .BindModelId<Symbol>();
    }
}